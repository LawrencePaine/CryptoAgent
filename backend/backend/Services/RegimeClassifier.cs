using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Services;

public class RegimeClassifier
{
    private readonly RegimeConfig _config;

    public RegimeClassifier(RegimeConfig config)
    {
        _config = config;
    }

    public RegimeStateEntity Classify(HourlyFeatureEntity feature)
    {
        if (!feature.IsComplete || feature.Sma168 == 0)
        {
            return Build(feature, "Unknown", "Insufficient data", 0.4m);
        }

        var isTrendUp = feature.TrendStrength >= _config.TrendStrengthThreshold;
        var isHighVol = feature.Vol24h >= _config.HighVol24hThreshold;
        var isDrawdown = feature.Drawdown7d <= _config.DrawdownRecoveringThreshold;
        var isRecovering = isDrawdown && feature.TrendStrength >= _config.RecoveryTrendStrengthThreshold;

        if (isRecovering)
        {
            return Build(feature, "Drawdown_Recovering",
                $"Drawdown7d={feature.Drawdown7d:F3} <= {_config.DrawdownRecoveringThreshold:F3} and TrendStrength={feature.TrendStrength:F3} >= {_config.RecoveryTrendStrengthThreshold:F3}",
                CalculateConfidence(feature));
        }

        if (isTrendUp && !isHighVol)
        {
            return Build(feature, "TrendUp_LowVol",
                $"TrendStrength={feature.TrendStrength:F3} >= {_config.TrendStrengthThreshold:F3} and Vol24h={feature.Vol24h:F3} < {_config.HighVol24hThreshold:F3}",
                CalculateConfidence(feature));
        }

        if (isTrendUp && isHighVol)
        {
            return Build(feature, "TrendUp_HighVol",
                $"TrendStrength={feature.TrendStrength:F3} >= {_config.TrendStrengthThreshold:F3} and Vol24h={feature.Vol24h:F3} >= {_config.HighVol24hThreshold:F3}",
                CalculateConfidence(feature));
        }

        if (!isTrendUp && !isHighVol)
        {
            return Build(feature, "Range_LowVol",
                $"TrendStrength={feature.TrendStrength:F3} < {_config.TrendStrengthThreshold:F3} and Vol24h={feature.Vol24h:F3} < {_config.HighVol24hThreshold:F3}",
                CalculateConfidence(feature));
        }

        return Build(feature, "Range_HighVol",
            $"TrendStrength={feature.TrendStrength:F3} < {_config.TrendStrengthThreshold:F3} and Vol24h={feature.Vol24h:F3} >= {_config.HighVol24hThreshold:F3}",
            CalculateConfidence(feature));
    }

    private static RegimeStateEntity Build(HourlyFeatureEntity feature, string regime, string reason, decimal confidence)
    {
        return new RegimeStateEntity
        {
            Asset = feature.Asset,
            HourUtc = feature.HourUtc,
            Regime = regime,
            Reason = reason,
            Confidence = confidence
        };
    }

    private decimal CalculateConfidence(HourlyFeatureEntity feature)
    {
        decimal trendDistance = Math.Abs(feature.TrendStrength - _config.TrendStrengthThreshold);
        decimal volDistance = Math.Abs(feature.Vol24h - _config.HighVol24hThreshold);
        var conf = 0.4m + Math.Min(0.5m, (trendDistance + volDistance) * 2);
        return Math.Min(0.9m, conf);
    }
}
