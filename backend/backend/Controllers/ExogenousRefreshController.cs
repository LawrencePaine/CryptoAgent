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
    public async Task<ActionResult<ExogenousRefreshResponse>> StartRefresh(CancellationToken ct)
    {
        var result = await _refreshService.RefreshAsync(ct);
        if (result.Status == ExogenousRefreshStatus.Running)
        {
            return Conflict(result);
        }

        return Ok(result);
    }
}
