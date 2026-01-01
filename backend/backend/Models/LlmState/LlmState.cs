using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Models.LlmState;

public class LlmState
{
    public DateTime TimestampUtc { get; set; }
    public string Mode { get; set; } = "PAPER"; // "PAPER" | "LIVE"

    public LlmPortfolioState Portfolio { get; set; } = new();
    public LlmMarketState Market { get; set; } = new();
    public LlmRiskState Risk { get; set; } = new();
    public LlmExogenousState? Exogenous { get; set; }

    public List<LlmRecentTrade> RecentTrades { get; set; } = new();
    public List<LlmRecentDecision> RecentDecisions { get; set; } = new();

    // Optional: fill later when analytics exist
    public LlmIndicatorsState? Indicators { get; set; }
}

public class LlmPortfolioState
{
    public decimal CashGbp { get; set; }
    public decimal VaultGbp { get; set; }
    public decimal BtcAmount { get; set; }
    public decimal EthAmount { get; set; }
    public decimal BtcCostBasisGbp { get; set; }
    public decimal EthCostBasisGbp { get; set; }

    public decimal BtcValueGbp { get; set; }
    public decimal EthValueGbp { get; set; }
    public decimal TotalValueGbp { get; set; }
    public decimal BtcUnrealisedPnlGbp { get; set; }
    public decimal EthUnrealisedPnlGbp { get; set; }
    public decimal BtcUnrealisedPnlPct { get; set; }
    public decimal EthUnrealisedPnlPct { get; set; }

    public decimal BtcAllocationPct { get; set; }
    public decimal EthAllocationPct { get; set; }
    public decimal CashAllocationPct { get; set; }
    public decimal VaultAllocationPct { get; set; }
}

public class LlmMarketState
{
    public decimal BtcPriceGbp { get; set; }
    public decimal EthPriceGbp { get; set; }

    public decimal BtcChange24hPct { get; set; }
    public decimal EthChange24hPct { get; set; }
    public decimal BtcChange7dPct { get; set; }
    public decimal EthChange7dPct { get; set; }
}

public class LlmRiskState
{
    public decimal MaxBtcAllocationPct { get; set; }
    public decimal MaxEthAllocationPct { get; set; }
    public decimal MinCashAllocationPct { get; set; }
    public decimal MaxTradeSizeGbp { get; set; }
    public int MaxTradesPerDay { get; set; }
    public int TradesToday { get; set; }

    public decimal TakerFeePct { get; set; } // e.g. 0.0050
}

public class LlmRecentTrade
{
    public DateTime TimestampUtc { get; set; }
    public string Asset { get; set; } = "";   // "BTC" | "ETH"
    public string Action { get; set; } = "";  // "BUY" | "SELL"
    public decimal AssetAmount { get; set; }
    public decimal SizeGbp { get; set; }
    public decimal PriceGbp { get; set; }
    public decimal FeeGbp { get; set; }
    public string Mode { get; set; } = "PAPER";
}

public class LlmRecentDecision
{
    public DateTime TimestampUtc { get; set; }
    public string ProviderUsed { get; set; } = ""; // "Local" | "OpenAI"
    public string FinalAction { get; set; } = "";  // "Buy"|"Sell"|"Hold"
    public string FinalAsset { get; set; } = "";   // "Btc"|"Eth"|"None"
    public decimal FinalSizeGbp { get; set; }
    public bool Executed { get; set; }
    public string RationaleShort { get; set; } = "";
    public string RiskReason { get; set; } = "";
}

// Placeholder for later
public class LlmIndicatorsState
{
    public decimal? BtcRsi14 { get; set; }
    public decimal? BtcSma50 { get; set; }
    public decimal? BtcMacdHist { get; set; }

    public decimal? EthRsi14 { get; set; }
    public decimal? EthSma50 { get; set; }
    public decimal? EthMacdHist { get; set; }
}

public class LlmExogenousState
{
    public Dictionary<string, decimal> ThemeScores { get; set; } = new();
    public Dictionary<string, string> AlignmentFlags { get; set; } = new();
    public decimal AbstainModifier { get; set; }
    public decimal ConfidenceThresholdModifier { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> TraceIds { get; set; } = new();
}
