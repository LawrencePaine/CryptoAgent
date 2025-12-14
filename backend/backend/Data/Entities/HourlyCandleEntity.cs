namespace CryptoAgent.Api.Data.Entities;

public class HourlyCandleEntity
{
    public int Id { get; set; }
    public string Asset { get; set; } = string.Empty;
    public DateTime HourUtc { get; set; }
    public decimal OpenGbp { get; set; }
    public decimal HighGbp { get; set; }
    public decimal LowGbp { get; set; }
    public decimal CloseGbp { get; set; }
    public decimal? Volume { get; set; }
    public string Source { get; set; } = string.Empty;
}
