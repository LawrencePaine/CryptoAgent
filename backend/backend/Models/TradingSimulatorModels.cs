namespace CryptoAgent.Api.Models;

public class ManualTradeRequest
{
    public string Asset { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public decimal SizeGbp { get; set; }
}

public class ManualTradeResponse
{
    public PortfolioDto Portfolio { get; set; } = new();
    public Trade Trade { get; set; } = new();
    public MarketSnapshot Market { get; set; } = new();
}

public class BookPerformanceSummary
{
    public decimal Equity { get; set; }
    public decimal NetProfit { get; set; }
    public decimal NetProfitPct { get; set; }
    public decimal Fees { get; set; }
    public decimal MaxDrawdownPct { get; set; }
    public int TradeCount { get; set; }
}

public class PerformanceCompareResponse
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public BookPerformanceSummary Agent { get; set; } = new();
    public BookPerformanceSummary Manual { get; set; } = new();
}
