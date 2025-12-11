using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly AgentService _agentService;

    public DashboardController(
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        AgentService agentService)
    {
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _agentService = agentService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
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
