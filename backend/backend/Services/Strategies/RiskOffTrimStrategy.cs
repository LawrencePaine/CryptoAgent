using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Services.Strategies;

public class RiskOffTrimStrategy : IStrategyModule
{
    public string Name => "RiskOffTrim";

    public Task<StrategySignal> EvaluateAsync(string asset, HourlyFeatureEntity features, RegimeStateEntity regime, Portfolio portfolio, RiskConfig risk, CancellationToken ct)
    {
        if (regime.Regime != "TrendUp_HighVol")
        {
            return Task.FromResult(Hold(asset, features));
        }

        var allocation = 0m;
        var assetValue = asset == "BTC" ? portfolio.BtcAmount * portfolio.BtcValueGbp : portfolio.EthAmount * portfolio.EthValueGbp;
        if (portfolio.TotalValueGbp > 0)
        {
            allocation = assetValue / portfolio.TotalValueGbp;
        }

        if (allocation > 0.5m * (asset == "BTC" ? risk.MaxBtcAllocationPct : risk.MaxEthAllocationPct) && features.Return24h > 0.03m)
        {
            var score = -0.5m;
            var size = Math.Min(risk.MaxTradeSizeGbp, assetValue * 0.10m);
            return Task.FromResult(new StrategySignal
            {
                Asset = asset,
                HourUtc = features.HourUtc,
                StrategyName = Name,
                SignalScore = score,
                SuggestedAction = "Sell",
                SuggestedSizeGbp = size,
                Reason = $"High vol trend with allocation {allocation:P1} and return24h {features.Return24h:P2}"
            });
        }

        return Task.FromResult(Hold(asset, features));
    }

    private StrategySignal Hold(string asset, HourlyFeatureEntity feature)
    {
        return new StrategySignal
        {
            Asset = asset,
            HourUtc = feature.HourUtc,
            StrategyName = Name,
            SignalScore = 0,
            SuggestedAction = "Hold",
            SuggestedSizeGbp = 0,
            Reason = "Regime inactive"
        };
    }
}
