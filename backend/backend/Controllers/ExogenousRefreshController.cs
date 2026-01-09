using CryptoAgent.Api.Models;
using CryptoAgent.Api.Services.Exogenous;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/exogenous/refresh")]
public class ExogenousRefreshController : ControllerBase
{
    private readonly ExogenousRefreshService _refreshService;

    public ExogenousRefreshController(ExogenousRefreshService refreshService)
    {
        _refreshService = refreshService;
    }

    [HttpPost]
    public async Task<ActionResult<object>> StartRefresh(CancellationToken ct)
    {
        var result = await _refreshService.StartRefreshAsync(ct);
        if (result.Status == ExogenousRefreshStartStatus.AlreadyRunning)
        {
            return Conflict(new { status = result.Status.ToString() });
        }

        if (result.Status == ExogenousRefreshStartStatus.Cooldown)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                status = result.Status.ToString(),
                nextAvailableUtc = result.NextAvailableUtc
            });
        }

        return Ok(new { jobId = result.JobId });
    }

    [HttpGet("{jobId}")]
    public ActionResult<ExogenousRefreshJobStatus> GetStatus(string jobId)
    {
        var status = _refreshService.GetStatus(jobId);
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }
}
