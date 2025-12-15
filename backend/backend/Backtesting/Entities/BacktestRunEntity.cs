namespace CryptoAgent.Api.Backtesting.Entities;

public class BacktestRunEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mode { get; set; } = "PAPER";
    public DateTime CreatedUtc { get; set; }
    public DateTime StartHourUtc { get; set; }
    public DateTime EndHourUtc { get; set; }
    public int WarmupHours { get; set; } = 168;
    public decimal FeePct { get; set; }
    public decimal SlippagePct { get; set; }
    public decimal InitialCashGbp { get; set; }
    public decimal MaxTradeSizeGbp { get; set; }
    public int MaxTradesPerDay { get; set; }
    public decimal MaxBtcAllocationPct { get; set; }
    public decimal MaxEthAllocationPct { get; set; }
    public decimal MinCashAllocationPct { get; set; }
    public int DecisionCadenceHours { get; set; } = 1;
    public string SelectorMode { get; set; } = "Deterministic";
    public string? Notes { get; set; }
}
