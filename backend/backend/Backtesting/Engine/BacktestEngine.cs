using CryptoAgent.Api.Backtesting.Entities;

namespace CryptoAgent.Api.Backtesting.Engine;

public record BacktestConfig
(
    DateTime StartHourUtc,
    DateTime EndHourUtc,
    int WarmupHours,
    decimal InitialCashGbp,
    decimal FeePct,
    decimal SlippagePct,
    decimal MaxTradeSizeGbp,
    int DecisionCadenceHours,
    string SelectorMode
);

public record BacktestResult(int RunId, BacktestMetricEntity? Metrics);

public class BacktestEngine
{
    /// <summary>
    /// Placeholder runner. Full deterministic implementation will iterate hourly history and populate persistence models.
    /// </summary>
    public Task<BacktestResult> RunAsync(BacktestConfig config, CancellationToken cancellationToken = default)
    {
        // In v1 we only record the run metadata; the execution loop will be added in subsequent iterations.
        return Task.FromResult(new BacktestResult(0, null));
    }
}

public record BacktestDecision(string Action, string Asset, decimal SizeGbp, bool Executed, string RiskReason);

public record BacktestSelectorContext(
    decimal CashGbp,
    decimal BtcAmount,
    decimal EthAmount,
    decimal BtcScore,
    decimal EthScore,
    decimal BtcSuggestedSize,
    decimal EthSuggestedSize,
    decimal MaxTradeSizeGbp,
    decimal MaxCashBuyFraction,
    decimal MinCashAllocationPct,
    decimal MaxBtcAllocationPct,
    decimal MaxEthAllocationPct
);

public interface IBacktestSelector
{
    BacktestDecision Select(BacktestSelectorContext ctx);
}

public class DeterministicBacktestSelector : IBacktestSelector
{
    public BacktestDecision Select(BacktestSelectorContext ctx)
    {
        var bestAsset = Math.Abs(ctx.BtcScore) >= Math.Abs(ctx.EthScore) ? "Btc" : "Eth";
        var bestScore = bestAsset == "Btc" ? ctx.BtcScore : ctx.EthScore;
        if (Math.Abs(bestScore) < 0.35m)
        {
            return new BacktestDecision("Hold", "None", 0, false, "Score below threshold");
        }

        var suggestedSize = bestAsset == "Btc" ? ctx.BtcSuggestedSize : ctx.EthSuggestedSize;
        var cappedSize = Math.Min(suggestedSize, ctx.MaxTradeSizeGbp);
        if (bestScore > 0)
        {
            var cashCap = ctx.CashGbp * ctx.MaxCashBuyFraction;
            cappedSize = Math.Min(cappedSize, cashCap);
            if (ctx.CashGbp <= 0 || cappedSize <= 0)
            {
                return new BacktestDecision("Hold", "None", 0, false, "Insufficient cash");
            }
            return new BacktestDecision("Buy", bestAsset, cappedSize, true, string.Empty);
        }

        // Sell path: ensure holdings exist
        var available = bestAsset == "Btc" ? ctx.BtcAmount : ctx.EthAmount;
        if (available <= 0)
        {
            return new BacktestDecision("Hold", "None", 0, false, "No holdings");
        }

        return new BacktestDecision("Sell", bestAsset, cappedSize, true, string.Empty);
    }
}
