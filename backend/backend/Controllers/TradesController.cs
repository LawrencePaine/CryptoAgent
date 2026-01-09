using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly PortfolioRepository _portfolioRepository;

    public TradesController(PortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Trade>>> GetTrades([FromQuery] string book = "AGENT", [FromQuery] int take = 20)
    {
        if (!Enum.TryParse<PortfolioBook>(book, true, out var bookEnum))
        {
            return BadRequest("book must be AGENT or MANUAL");
        }

        var trades = await _portfolioRepository.GetRecentTradesAsync(take, bookEnum);
        return Ok(trades);
    }
}
