using CryptoAgent.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/decision-inputs")]
public class DecisionInputsController : ControllerBase
{
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;

    public DecisionInputsController(DecisionInputsExogenousRepository decisionInputsRepository)
    {
        _decisionInputsRepository = decisionInputsRepository;
    }

    [HttpGet("exogenous")]
    public async Task<ActionResult> GetExogenous([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var inputs = await _decisionInputsRepository.GetRangeAsync(from, to);
        var dtos = inputs.Select(DecisionInputsExogenousRepository.ToDto).ToList();
        return Ok(dtos);
    }
}
