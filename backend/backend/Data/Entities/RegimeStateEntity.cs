namespace CryptoAgent.Api.Data.Entities;

public class RegimeStateEntity
{
    public int Id { get; set; }
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public string Regime { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public decimal? Confidence { get; set; }
}
