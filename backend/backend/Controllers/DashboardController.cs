using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly AgentService _agentService;
    private readonly DecisionRepository _decisionRepository;

    public DashboardController(
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        AgentService agentService,
        DecisionRepository decisionRepository)
    {
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _agentService = agentService;
        _decisionRepository = decisionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> GetDashboard()
    {
        var portfolio = await _portfolioRepository.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();
        var recentTrades = await _portfolioRepository.GetRecentTradesAsync(20);
        var recentDecisions = await _decisionRepository.GetRecentDecisionsAsync(10);
        var lastDecision = recentDecisions.FirstOrDefault() ?? _agentService.LastDecision;
        var dto = portfolio.ToDto(market);

        var response = new DashboardResponse
        {
            Portfolio = dto,
            Market = market,
            LastDecision = lastDecision,
            RecentTrades = recentTrades,
            RecentDecisions = recentDecisions,
            PositionCommentary = GenerateCommentary(dto, lastDecision, recentTrades)
        };

        return Ok(response);
    }
    private static string GenerateCommentary(PortfolioDto p, LastDecision? lastDecision, List<Trade> recentTrades)
    {
        var holdings =
            $"Holdings: £{p.CashGbp:N2} cash, £{p.VaultGbp:N2} vault, £{p.BtcValueGbp:N2} BTC ({p.BtcAllocationPct:P0}), £{p.EthValueGbp:N2} ETH ({p.EthAllocationPct:P0}).";

        string decisionText;
        if (lastDecision != null)
        {
            var action = lastDecision.FinalAction.ToString().ToUpperInvariant();
            var asset = lastDecision.FinalAsset.ToString().ToUpperInvariant();
            var execution = lastDecision.Executed ? "executed" : "not executed";
            decisionText =
                $"Last decision: {action} £{lastDecision.FinalSizeGbp:N2} {asset} {execution}.";

            if (!string.IsNullOrWhiteSpace(lastDecision.RationaleShort))
            {
                decisionText += $" Reason: {lastDecision.RationaleShort}";
            }

            if (!string.IsNullOrWhiteSpace(lastDecision.RiskReason))
            {
                decisionText += $" (Risk: {lastDecision.RiskReason})";
            }
        }
        else
        {
            decisionText = "Last decision: none recorded.";
        }

        var tradeSnippets = recentTrades
            .Take(3)
            .Select(t => $"{t.Action.ToString().ToUpperInvariant()} £{t.SizeGbp:N2} {(t.Asset == AssetType.Btc ? "BTC" : "ETH")}")
            .ToList();

        var tradesText = tradeSnippets.Count > 0
            ? $"Recent trades: {string.Join(", ", tradeSnippets)}."
            : "Recent trades: none.";

        return string.Join(" ", holdings, decisionText, tradesText);
    }
}
