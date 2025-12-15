namespace CryptoAgent.Api.Backtesting.Entities;

public class BacktestTradeEntity
{
    public int Id { get; set; }
    public int BacktestRunId { get; set; }
    public DateTime HourUtc { get; set; }
    public string Asset { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public decimal SizeGbp { get; set; }
    public decimal PriceGbp { get; set; }
    public decimal FeeGbp { get; set; }
    public decimal SlippageGbp { get; set; }
}
