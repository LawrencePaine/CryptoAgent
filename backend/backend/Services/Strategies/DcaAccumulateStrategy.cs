using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Services.Strategies;

public class DcaAccumulateStrategy : IStrategyModule
{
    public string Name => "DcaAccumulate";

    public Task<StrategySignal> EvaluateAsync(string asset, HourlyFeatureEntity features, RegimeStateEntity regime, Portfolio portfolio, RiskConfig risk, CancellationToken ct)
    {
        if (regime.Regime is not ("TrendUp_LowVol" or "Range_LowVol"))
        {
            return Task.FromResult(Hold(asset, features));
        }

        var portfolioValue = portfolio.TotalValueGbp > 0 ? portfolio.TotalValueGbp : 1;
        var assetValue = asset == "BTC" ? portfolio.BtcAmount * portfolio.BtcValueGbp / (portfolio.BtcAmount == 0 ? 1 : portfolio.BtcAmount) : portfolio.EthAmount * portfolio.EthValueGbp / (portfolio.EthAmount == 0 ? 1 : portfolio.EthAmount);
        var allocation = assetValue / portfolioValue;

        if (regime.Regime is "TrendUp_LowVol" or "Range_LowVol")
        {
            var tradesToday = 0;
            if (tradesToday >= risk.MaxTradesPerDay)
            {
                return Task.FromResult(Hold(asset, features, "Max trades reached"));
            }

            if (allocation >= (asset == "BTC" ? risk.MaxBtcAllocationPct : risk.MaxEthAllocationPct))
            {
                return Task.FromResult(Hold(asset, features, "Allocation high"));
            }

            var baseline = 0.2m;
            if (features.Return24h < 0) baseline += 0.2m;
            var score = Math.Min(0.6m, baseline);

            var suggestedSize = Math.Min(risk.MaxTradeSizeGbp, portfolio.CashGbp * 0.10m);
            return Task.FromResult(new StrategySignal
            {
                Asset = asset,
                HourUtc = features.HourUtc,
                StrategyName = Name,
                SignalScore = score,
                SuggestedAction = score >= 0.35m ? "Buy" : "Hold",
                SuggestedSizeGbp = suggestedSize,
                Reason = $"Regime {regime.Regime}, alloc={allocation:P1}, cash=£{portfolio.CashGbp:N0}, return24h={features.Return24h:P2}"
            });
        }

        return Task.FromResult(Hold(asset, features));
    }

    private StrategySignal Hold(string asset, HourlyFeatureEntity feature, string reason = "Regime inactive")
    {
        return new StrategySignal
        {
            Asset = asset,
            HourUtc = feature.HourUtc,
            StrategyName = Name,
            SignalScore = 0,
            SuggestedAction = "Hold",
            SuggestedSizeGbp = 0,
            Reason = reason
        };
    }
}
