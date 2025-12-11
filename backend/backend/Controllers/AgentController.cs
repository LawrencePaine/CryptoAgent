using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;

    public AgentController(
        AgentService agentService,
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService)
    {
        _agentService = agentService;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
    }

    [HttpPost("run-once")]
    public async Task<ActionResult<DashboardResponse>> RunOnce()
    {
        await _agentService.RunOnceAsync();

        // Return updated dashboard
        var portfolio = await _portfolioRepository.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();
        var recentTrades = await _portfolioRepository.GetRecentTradesAsync(20);

        var response = new DashboardResponse
        {
            Portfolio = portfolio.ToDto(market),
            Market = market,
            LastDecision = _agentService.LastDecision,
            RecentTrades = recentTrades
        };

        return Ok(response);
    }
}
