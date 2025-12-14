using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Services.Strategies;

public class MeanReversionStrategy : IStrategyModule
{
    public string Name => "MeanReversion";

    public Task<StrategySignal> EvaluateAsync(string asset, HourlyFeatureEntity features, RegimeStateEntity regime, Portfolio portfolio, RiskConfig risk, CancellationToken ct)
    {
        if (regime.Regime is not ("Range_HighVol" or "Drawdown_Recovering"))
        {
            return Task.FromResult(Hold(asset, features));
        }

        var score = 0m;
        var action = "Hold";
        decimal size = 0;

        if (features.Return24h <= -0.03m)
        {
            score = Math.Min(0.7m, Math.Clamp((-features.Return24h) / 0.10m, 0, 1) * 0.7m);
            action = "Buy";
            size = Math.Min(risk.MaxTradeSizeGbp, portfolio.CashGbp * 0.08m);
        }
        else if (features.Return24h >= 0.03m)
        {
            score = -Math.Min(0.7m, Math.Clamp((features.Return24h) / 0.10m, 0, 1) * 0.7m);
            action = "Sell";
            var price = 1m; // placeholder sizing, will be clamped by risk engine
            size = Math.Min(risk.MaxTradeSizeGbp, price * 0.10m);
        }

        return Task.FromResult(new StrategySignal
        {
            Asset = asset,
            HourUtc = features.HourUtc,
            StrategyName = Name,
            SignalScore = score,
            SuggestedAction = action,
            SuggestedSizeGbp = size,
            Reason = $"Regime {regime.Regime}, return24h={features.Return24h:P2}, vol24h={features.Vol24h:P2}"
        });
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
