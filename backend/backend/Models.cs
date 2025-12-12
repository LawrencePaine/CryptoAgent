using System.Text.Json.Serialization;

namespace CryptoAgent.Api.Models;

public class Portfolio
{
    public decimal CashGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }

    // Profit "vault" - off-limits to trading
    public decimal VaultGbp { get; set; }

    // High water mark of total equity achieved
    public decimal HighWatermarkGbp { get; set; }

    // Derived / transient fields (not persisted)
    [JsonIgnore]
    public decimal BtcValueGbp { get; set; }
    [JsonIgnore]
    public decimal EthValueGbp { get; set; }
    [JsonIgnore]
    public decimal TotalValueGbp { get; set; }
    [JsonIgnore]
    public decimal BtcAllocationPct { get; set; }
    [JsonIgnore]
    public decimal EthAllocationPct { get; set; }
    [JsonIgnore]
    public decimal CashAllocationPct { get; set; }
}

public class MarketSnapshot
{
    public DateTime TimestampUtc { get; set; }

    public decimal BtcPriceGbp { get; set; }
    public decimal EthPriceGbp { get; set; }

    public decimal BtcChange24hPct { get; set; }
    public decimal EthChange24hPct { get; set; }
    public decimal BtcChange7dPct { get; set; }
    public decimal EthChange7dPct { get; set; }

    public TechnicalAnalysis? BtcTechnical { get; set; }
    public TechnicalAnalysis? EthTechnical { get; set; }
}

public class TechnicalAnalysis
{
    public decimal Rsi14 { get; set; }
    public decimal Sma50 { get; set; }
    public decimal MacdValue { get; set; }
    public decimal MacdSignal { get; set; }
    public decimal MacdHistogram { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RawActionType { Buy, Sell, Hold }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssetType { Btc, Eth, None }

public class Trade
{
    public DateTime TimestampUtc { get; set; }
    public AssetType Asset { get; set; }
    public RawActionType Action { get; set; }
    public decimal SizeGbp { get; set; }
    public decimal PriceGbp { get; set; }
    public decimal FeeGbp { get; set; }
    public string Mode { get; set; } = "PAPER";
}

public class LastDecision
{
    public DateTime TimestampUtc { get; set; }
    public RawActionType LlmAction { get; set; }
    public AssetType LlmAsset { get; set; }
    public decimal LlmSizeGbp { get; set; }

    public RawActionType FinalAction { get; set; }
    public AssetType FinalAsset { get; set; }
    public decimal FinalSizeGbp { get; set; }

    public bool Executed { get; set; }
    public string RiskReason { get; set; } = string.Empty;
    public string RationaleShort { get; set; } = string.Empty;
    public string Mode { get; set; } = "PAPER";
}

// LLM suggestion
public class LlmSuggestion
{
    public RawActionType Action { get; set; }
    public AssetType Asset { get; set; }
    public decimal SizeGbp { get; set; }
    public decimal Confidence { get; set; }
    public string RationaleShort { get; set; } = string.Empty;
    public string RationaleDetailed { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

// DTO for portfolio + derived values
public class PortfolioDto
{
    public decimal CashGbp { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }
    public decimal BtcValueGbp { get; set; }
    public decimal EthValueGbp { get; set; }
    public decimal TotalValueGbp { get; set; }
    public decimal BtcAllocationPct { get; set; }
    public decimal EthAllocationPct { get; set; }
    public decimal CashAllocationPct { get; set; }
}

public class DashboardResponse
{
    public PortfolioDto Portfolio { get; set; } = null!;
    public MarketSnapshot Market { get; set; } = null!;
    public LastDecision? LastDecision { get; set; }
    public List<Trade> RecentTrades { get; set; } = new();
    public List<LastDecision> RecentDecisions { get; set; } = new();
    public string PositionCommentary { get; set; } = string.Empty;
}

public class PerformanceSnapshot
{
    public DateTime DateUtc { get; set; }
    public decimal PortfolioValueGbp { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal NetDepositsGbp { get; set; }
    public decimal CumulatedAiCostGbp { get; set; }
    public decimal CumulatedFeesGbp { get; set; }
}

// App-level config
public class RiskConfig
{
    public decimal MaxBtcAllocationPct { get; set; } = 0.7m;
    public decimal MaxEthAllocationPct { get; set; } = 0.7m;
    public decimal MinCashAllocationPct { get; set; } = 0.1m;
    public decimal MaxTradeSizeGbp { get; set; } = 5m;
    public int MaxTradesPerDay { get; set; } = 2;
    public decimal MaxBuyAfter7dRallyPct { get; set; } = 0.15m;
}

public class AppConfig
{
    public decimal EstimatedAiCostPerRunGbp { get; set; } = 0.02m;
    public decimal MonthlyAiFixedCostGbp { get; set; } = 0m;
    public AgentMode Mode { get; set; } = AgentMode.Paper;
}

public class FeeConfig
{
    public decimal MakerPct { get; set; } = 0.0025m;
    public decimal TakerPct { get; set; } = 0.0050m;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentMode
{
    Paper,
    Live
}

public static class PortfolioExtensions
{
    public static PortfolioDto ToDto(this Portfolio portfolio, MarketSnapshot market)
    {
        var btcVal = portfolio.BtcAmount * market.BtcPriceGbp;
        var ethVal = portfolio.EthAmount * market.EthPriceGbp;
        var totalVal = portfolio.CashGbp + portfolio.VaultGbp + btcVal + ethVal;

        return new PortfolioDto
        {
            CashGbp = portfolio.CashGbp,
            VaultGbp = portfolio.VaultGbp,
            BtcAmount = portfolio.BtcAmount,
            EthAmount = portfolio.EthAmount,
            BtcValueGbp = btcVal,
            EthValueGbp = ethVal,
            TotalValueGbp = totalVal,
            BtcAllocationPct = totalVal > 0 ? btcVal / totalVal : 0,
            EthAllocationPct = totalVal > 0 ? ethVal / totalVal : 0,
            CashAllocationPct = totalVal > 0 ? (portfolio.CashGbp + portfolio.VaultGbp) / totalVal : 0
        };
    }
}
