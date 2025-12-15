using CryptoAgent.Api.Backtesting.Entities;

namespace CryptoAgent.Api.Backtesting.Metrics;

public class MetricsCalculator
{
    public BacktestMetricEntity BuildSummary(IEnumerable<BacktestStepEntity> steps, IEnumerable<BacktestTradeEntity> trades)
    {
        var equity = steps.OrderBy(s => s.HourUtc).Select(s => s.TotalValueGbp).ToList();
        var finalValue = equity.LastOrDefault();
        var startValue = equity.FirstOrDefault();
        var netProfit = finalValue - startValue;
        var netProfitPct = startValue == 0 ? 0 : netProfit / startValue;
        var maxDrawdown = CalculateMaxDrawdown(equity);
        return new BacktestMetricEntity
        {
            FinalValueGbp = finalValue,
            NetProfitGbp = netProfit,
            NetProfitPct = netProfitPct,
            MaxDrawdownPct = maxDrawdown,
            TradeCount = trades.Count(),
            FeesPaidGbp = trades.Sum(t => t.FeeGbp),
            SlippagePaidGbp = trades.Sum(t => t.SlippageGbp)
        };
    }

    private static decimal CalculateMaxDrawdown(List<decimal> equity)
    {
        if (equity.Count == 0)
        {
            return 0;
        }

        decimal peak = equity[0];
        decimal maxDd = 0;
        foreach (var value in equity)
        {
            if (value > peak)
            {
                peak = value;
            }
            if (peak > 0)
            {
                var dd = (value / peak) - 1;
                if (dd < maxDd)
                {
                    maxDd = dd;
                }
            }
        }

        return maxDd;
    }
}
