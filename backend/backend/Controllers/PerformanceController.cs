using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly PerformanceRepository _performanceRepository;
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;

    public PerformanceController(
        PerformanceRepository performanceRepository,
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService)
    {
        _performanceRepository = performanceRepository;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
    }

    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<object>>> GetMonthlyPerformance()
    {
        var all = await _performanceRepository.GetAllAsync();

        var groups = all.GroupBy(x => new { x.DateUtc.Year, x.DateUtc.Month })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month)
                        .Select(g =>
                        {
                            var first = g.First();
                            var last = g.Last();
                            var pnl = last.PortfolioValueGbp - first.PortfolioValueGbp;
                            var aiCost = last.CumulatedAiCostGbp - first.CumulatedAiCostGbp;
                            var fees = last.CumulatedFeesGbp - first.CumulatedFeesGbp;

                            return new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                StartValue = first.PortfolioValueGbp,
                                EndValue = last.PortfolioValueGbp,
                                PnlGbp = pnl,
                                AiCostGbp = aiCost,
                                FeesGbp = fees,
                                NetAfterAiAndFeesGbp = pnl - aiCost - fees,
                                VaultEndGbp = last.VaultGbp
                            };
                        })
                        .ToList();

        return Ok(groups);
    }

    [HttpGet("compare")]
    public async Task<ActionResult<PerformanceCompareResponse>> ComparePerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var now = DateTime.UtcNow;
        var fromUtc = from?.ToUniversalTime() ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = to?.ToUniversalTime() ?? now;

        if (toUtc <= fromUtc)
        {
            return BadRequest("to must be greater than from");
        }

        var market = await _marketDataService.GetSnapshotAsync();

        var agentSummary = await BuildSummaryAsync(PortfolioBook.Agent, fromUtc, toUtc, market);
        var manualSummary = await BuildSummaryAsync(PortfolioBook.Manual, fromUtc, toUtc, market);

        return Ok(new PerformanceCompareResponse
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Agent = agentSummary,
            Manual = manualSummary
        });
    }

    private async Task<BookPerformanceSummary> BuildSummaryAsync(
        PortfolioBook book,
        DateTime fromUtc,
        DateTime toUtc,
        MarketSnapshot market)
    {
        var trades = await _portfolioRepository.GetTradesUpToAsync(toUtc, book);

        var cash = 50m;
        decimal btcAmount = 0;
        decimal ethAmount = 0;
        decimal peak = 0;
        decimal maxDrawdown = 0;

        foreach (var trade in trades)
        {
            if (trade.Action == RawActionType.Buy)
            {
                cash -= trade.SizeGbp + trade.FeeGbp;
                if (trade.Asset == AssetType.Btc)
                {
                    btcAmount += trade.AssetAmount;
                }
                else if (trade.Asset == AssetType.Eth)
                {
                    ethAmount += trade.AssetAmount;
                }
            }
            else
            {
                cash += trade.SizeGbp - trade.FeeGbp;
                if (trade.Asset == AssetType.Btc)
                {
                    btcAmount -= trade.AssetAmount;
                }
                else if (trade.Asset == AssetType.Eth)
                {
                    ethAmount -= trade.AssetAmount;
                }
            }

            if (trade.TimestampUtc >= fromUtc)
            {
                var equity = cash +
                             btcAmount * (trade.Asset == AssetType.Btc ? trade.PriceGbp : market.BtcPriceGbp) +
                             ethAmount * (trade.Asset == AssetType.Eth ? trade.PriceGbp : market.EthPriceGbp);

                if (equity > peak)
                {
                    peak = equity;
                }
                else if (peak > 0)
                {
                    var drawdown = (peak - equity) / peak;
                    maxDrawdown = Math.Max(maxDrawdown, drawdown);
                }
            }
        }

        var equityNow = cash + btcAmount * market.BtcPriceGbp + ethAmount * market.EthPriceGbp;
        var netProfit = equityNow - 50m;
        var netProfitPct = 50m == 0 ? 0 : netProfit / 50m;

        var fees = trades.Where(t => t.TimestampUtc >= fromUtc && t.TimestampUtc <= toUtc).Sum(t => t.FeeGbp);
        var tradeCount = trades.Count(t => t.TimestampUtc >= fromUtc && t.TimestampUtc <= toUtc);

        return new BookPerformanceSummary
        {
            Equity = equityNow,
            NetProfit = netProfit,
            NetProfitPct = netProfitPct,
            Fees = fees,
            MaxDrawdownPct = maxDrawdown,
            TradeCount = tradeCount
        };
    }
}
