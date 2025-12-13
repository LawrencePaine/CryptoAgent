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
    private readonly DecisionRepository _decisionRepository;

    public AgentController(
        AgentService agentService,
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        DecisionRepository decisionRepository)
    {
        _agentService = agentService;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _decisionRepository = decisionRepository;
    }

    [HttpPost("run-once")]
    public async Task<ActionResult<DashboardResponse>> RunOnce()
    {
        await _agentService.RunOnceAsync();

        // Return updated dashboard
        var portfolio = await _portfolioRepository.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();
        var recentTrades = await _portfolioRepository.GetRecentTradesAsync(20);
        var recentDecisions = await _decisionRepository.GetRecentAsync(10);
        var dto = portfolio.ToDto(market);

        var response = new DashboardResponse
        {
            Portfolio = dto,
            Market = market,
            LastDecision = _agentService.LastDecision,
            RecentTrades = recentTrades,
            RecentDecisions = recentDecisions,
            PositionCommentary = GenerateCommentary(dto)
        };

        return Ok(response);
    }

    private static string GenerateCommentary(PortfolioDto p)
    {
        var topAsset = "Cash";
        var topPct = p.CashAllocationPct;

        if (p.BtcAllocationPct > topPct)
        {
            topAsset = "BTC";
            topPct = p.BtcAllocationPct;
        }
        if (p.EthAllocationPct > topPct)
        {
            topAsset = "ETH";
            topPct = p.EthAllocationPct;
        }

        return $"Portfolio is {(topPct * 100):F0}% {topAsset}. Total Value: £{p.TotalValueGbp:N2}.";
    }
}
