namespace CryptoAgent.Api.Models;

public class MarketContextSnapshot
{
    public string Asset { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string Regime { get; set; } = string.Empty;
    public decimal VolatilityScore { get; set; }
    public decimal TrendScore { get; set; }
    public decimal MomentumScore { get; set; }
}
