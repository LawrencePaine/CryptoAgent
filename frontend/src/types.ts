export type AssetPosition = {
  amount: number;
  costBasisGbp: number;
  currentValueGbp: number;
  unrealisedPnlGbp: number;
  unrealisedPnlPct: number;
  allocationPct: number;
};

export type PortfolioDto = {
  cashGbp: number;
  vaultGbp: number;
  totalValueGbp: number;
  cashAllocationPct: number;
  vaultAllocationPct: number;
  btc: AssetPosition;
  eth: AssetPosition;
};

export type MarketSnapshot = {
  timestampUtc: string;
  btcPriceGbp: number;
  ethPriceGbp: number;
  btcChange24hPct: number;
  ethChange24hPct: number;
  btcChange7dPct: number;
  ethChange7dPct: number;
};

export type LastDecision = {
  timestampUtc: string;
  llmAction: "Buy" | "Sell" | "Hold" | "BUY" | "SELL" | "HOLD";
  llmAsset: "Btc" | "Eth" | "None" | "BTC" | "ETH" | "NONE";
  llmSizeGbp: number;
  finalAction: "Buy" | "Sell" | "Hold" | "BUY" | "SELL" | "HOLD";
  finalAsset: "Btc" | "Eth" | "None" | "BTC" | "ETH" | "NONE";
  finalSizeGbp: number;
  btcValueGbp?: number;
  ethValueGbp?: number;
  totalValueGbp?: number;
  btcUnrealisedPnlGbp?: number;
  ethUnrealisedPnlGbp?: number;
  btcCostBasisGbp?: number;
  ethCostBasisGbp?: number;
  executed: boolean;
  riskReason: string;
  rationaleShort: string;
  rationaleDetailed: string;
  mode: "PAPER" | "LIVE";
};

export type Trade = {
  timestampUtc: string;
  asset: "Btc" | "Eth" | "BTC" | "ETH";
  action: "Buy" | "Sell" | "BUY" | "SELL";
  assetAmount: number;
  sizeGbp: number;
  priceGbp: number;
  mode: "PAPER" | "LIVE";
};

export type DashboardResponse = {
  portfolio: PortfolioDto;
  market: MarketSnapshot;
  lastDecision: LastDecision | null;
  recentTrades: Trade[];
  recentDecisions: LastDecision[];
  positionCommentary: string;
};

export type MonthlyPerformance = {
  year: number;
  month: number;
  startValue: number;
  endValue: number;
  pnlGbp: number;
  aiCostGbp: number;
  netAfterAiGbp: number;
  vaultEndGbp: number;
};
