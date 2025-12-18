using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly DecisionRepository _decisionRepository;
    private readonly PortfolioValuationService _valuationService;

    public AgentController(
        AgentService agentService,
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        DecisionRepository decisionRepository,
        PortfolioValuationService valuationService)
    {
        _agentService = agentService;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _decisionRepository = decisionRepository;
        _valuationService = valuationService;
    }

    [HttpPost("run-once")]
    public async Task<ActionResult<DashboardResponse>> RunOnce()
    {
        await _agentService.RunOnceAsync();

        // Return updated dashboard
        var portfolio = await _portfolioRepository.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();
        var recentTrades = await _portfolioRepository.GetRecentTradesAsync(20);
        var recentDecisions = await _decisionRepository.GetRecentDecisionsAsync(10);
        var lastDecision = recentDecisions.FirstOrDefault() ?? _agentService.LastDecision;
        var valuation = _valuationService.Calculate(portfolio, market);
        var dto = portfolio.ToDto(valuation);

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
            $"Holdings: £{p.CashGbp:N2} cash, £{p.VaultGbp:N2} vault, £{p.Btc.CurrentValueGbp:N2} BTC ({p.Btc.AllocationPct:P0}), £{p.Eth.CurrentValueGbp:N2} ETH ({p.Eth.AllocationPct:P0}).";

        var pnlText =
            $"BTC uPnL: {p.Btc.UnrealisedPnlGbp:+£0.00;-£0.00;+£0.00} ({p.Btc.UnrealisedPnlPct:P1}), ETH uPnL: {p.Eth.UnrealisedPnlGbp:+£0.00;-£0.00;+£0.00} ({p.Eth.UnrealisedPnlPct:P1}).";

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

        return string.Join(" ", holdings, pnlText, decisionText, tradesText);
    }
}
