namespace CryptoAgent.Api.Backtesting.Entities;

public class BacktestMetricEntity
{
    public int Id { get; set; }
    public int BacktestRunId { get; set; }
    public decimal FinalValueGbp { get; set; }
    public decimal NetProfitGbp { get; set; }
    public decimal NetProfitPct { get; set; }
    public decimal MaxDrawdownPct { get; set; }
    public int TradeCount { get; set; }
    public decimal? WinRatePct { get; set; }
    public decimal? AvgTradePnlGbp { get; set; }
    public decimal FeesPaidGbp { get; set; }
    public decimal SlippagePaidGbp { get; set; }
    public decimal? SharpeLike { get; set; }
    public decimal Baseline_Hodl_FinalValueGbp { get; set; }
    public decimal Baseline_Dca_FinalValueGbp { get; set; }
    public decimal Baseline_Rebalance_FinalValueGbp { get; set; }
}
