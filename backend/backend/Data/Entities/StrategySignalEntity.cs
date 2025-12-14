namespace CryptoAgent.Api.Data.Entities;

public class StrategySignalEntity
{
    public int Id { get; set; }
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public decimal SignalScore { get; set; }
    public string SuggestedAction { get; set; } = string.Empty;
    public decimal SuggestedSizeGbp { get; set; }
    public string Reason { get; set; } = string.Empty;
}
