using CryptoAgent.Api.Data.Entities;

namespace CryptoAgent.Api.Models;

public class HourlyCandle
{
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public decimal OpenGbp { get; set; }
    public decimal HighGbp { get; set; }
    public decimal LowGbp { get; set; }
    public decimal CloseGbp { get; set; }
    public decimal? Volume { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class StrategySignal
{
    public string StrategyName { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public decimal SignalScore { get; set; }
    public string SuggestedAction { get; set; } = string.Empty;
    public decimal SuggestedSizeGbp { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public interface IStrategyModule
{
    string Name { get; }
    Task<StrategySignal> EvaluateAsync(
        string asset,
        HourlyFeatureEntity features,
        RegimeStateEntity regime,
        Portfolio portfolio,
        RiskConfig risk,
        CancellationToken ct);
}

public class RegimeConfig
{
    public decimal TrendStrengthThreshold { get; set; } = 0.01m;
    public decimal HighVol24hThreshold { get; set; } = 0.015m;
    public decimal DrawdownRecoveringThreshold { get; set; } = -0.08m;
    public decimal RecoveryTrendStrengthThreshold { get; set; } = 0m;
}

public class WorkerConfig
{
    public int SnapshotMinutes { get; set; } = 10;
    public int HourlyCandleFetchMinute { get; set; } = 2;
    public int RunDecisionMinute { get; set; } = 5;
}
