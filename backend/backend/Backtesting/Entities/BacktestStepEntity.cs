namespace CryptoAgent.Api.Backtesting.Entities;

public class BacktestStepEntity
{
    public int Id { get; set; }
    public int BacktestRunId { get; set; }
    public DateTime HourUtc { get; set; }
    public decimal BtcCloseGbp { get; set; }
    public decimal EthCloseGbp { get; set; }
    public string BtcRegime { get; set; } = string.Empty;
    public string EthRegime { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty;
    public decimal SizeGbp { get; set; }
    public bool Executed { get; set; }
    public string RiskReason { get; set; } = string.Empty;
    public decimal CashGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal TotalValueGbp { get; set; }
}
