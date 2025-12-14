namespace CryptoAgent.Api.Data.Entities;

public class HourlyFeatureEntity
{
    public int Id { get; set; }
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public decimal Return1h { get; set; }
    public decimal Return24h { get; set; }
    public decimal Return7d { get; set; }
    public decimal Vol24h { get; set; }
    public decimal Vol72h { get; set; }
    public decimal Sma24 { get; set; }
    public decimal Sma168 { get; set; }
    public decimal TrendStrength { get; set; }
    public decimal Drawdown7d { get; set; }
    public decimal MomentumScore { get; set; }
    public bool IsComplete { get; set; }
}
