using CryptoAgent.Api.Models;
using CryptoAgent.Api.Models.LlmState;
using CryptoAgent.Api.Repositories;
using System.Linq;

namespace CryptoAgent.Api.Services;

public class LlmStateBuilder
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly DecisionRepository _decisionRepository;
    private readonly RiskConfig _riskConfig;
    private readonly FeeConfig _feeConfig;
    private readonly AppConfig _appConfig;

    public LlmStateBuilder(
        PortfolioRepository portfolioRepository,
        DecisionRepository decisionRepository,
        RiskConfig riskConfig,
        FeeConfig feeConfig,
        AppConfig appConfig)
    {
        _portfolioRepository = portfolioRepository;
        _decisionRepository = decisionRepository;
        _riskConfig = riskConfig;
        _feeConfig = feeConfig;
        _appConfig = appConfig;
    }

    public async Task<LlmState> BuildAsync(Portfolio portfolio, MarketSnapshot market)
    {
        var dto = portfolio.ToDto(market);

        var recentTrades = await _portfolioRepository.GetRecentTradesAsync(10);
        var tradesToday = await _portfolioRepository.CountTradesTodayAsync();
        var recentDecisions = await _decisionRepository.GetRecentDecisionsAsync(10);

        var state = new LlmState
        {
            TimestampUtc = DateTime.UtcNow,
            Mode = _appConfig.Mode == AgentMode.Live ? "LIVE" : "PAPER",
            Portfolio = new LlmPortfolioState
            {
                CashGbp = dto.CashGbp,
                VaultGbp = dto.VaultGbp,
                BtcAmount = dto.BtcAmount,
                EthAmount = dto.EthAmount,
                BtcValueGbp = dto.BtcValueGbp,
                EthValueGbp = dto.EthValueGbp,
                TotalValueGbp = dto.TotalValueGbp,
                BtcAllocationPct = dto.BtcAllocationPct,
                EthAllocationPct = dto.EthAllocationPct,
                CashAllocationPct = dto.CashAllocationPct
            },
            Market = new LlmMarketState
            {
                BtcPriceGbp = market.BtcPriceGbp,
                EthPriceGbp = market.EthPriceGbp,
                BtcChange24hPct = market.BtcChange24hPct,
                EthChange24hPct = market.EthChange24hPct,
                BtcChange7dPct = market.BtcChange7dPct,
                EthChange7dPct = market.EthChange7dPct
            },
            Risk = new LlmRiskState
            {
                MaxBtcAllocationPct = _riskConfig.MaxBtcAllocationPct,
                MaxEthAllocationPct = _riskConfig.MaxEthAllocationPct,
                MinCashAllocationPct = _riskConfig.MinCashAllocationPct,
                MaxTradeSizeGbp = _riskConfig.MaxTradeSizeGbp,
                MaxTradesPerDay = _riskConfig.MaxTradesPerDay,
                TradesToday = tradesToday,
                TakerFeePct = _feeConfig.TakerPct
            },
            Indicators = market.BtcTechnical != null || market.EthTechnical != null
                ? new LlmIndicatorsState
                {
                    BtcRsi14 = market.BtcTechnical?.Rsi14,
                    BtcSma50 = market.BtcTechnical?.Sma50,
                    BtcMacdHist = market.BtcTechnical?.MacdHistogram,
                    EthRsi14 = market.EthTechnical?.Rsi14,
                    EthSma50 = market.EthTechnical?.Sma50,
                    EthMacdHist = market.EthTechnical?.MacdHistogram
                }
                : null
        };

        state.RecentTrades.AddRange(recentTrades.Select(t => new LlmRecentTrade
        {
            TimestampUtc = t.TimestampUtc,
            Asset = t.Asset == AssetType.Btc ? "BTC" : "ETH",
            Action = t.Action == RawActionType.Buy ? "BUY" : "SELL",
            SizeGbp = t.SizeGbp,
            PriceGbp = t.PriceGbp,
            FeeGbp = t.FeeGbp,
            Mode = t.Mode
        }));

        state.RecentDecisions.AddRange(recentDecisions.Select(d => new LlmRecentDecision
        {
            TimestampUtc = d.TimestampUtc,
            ProviderUsed = d.ProviderUsed,
            FinalAction = d.FinalAction.ToString(),
            FinalAsset = d.FinalAsset.ToString(),
            FinalSizeGbp = d.FinalSizeGbp,
            Executed = d.Executed,
            RationaleShort = d.RationaleShort,
            RiskReason = d.RiskReason
        }));

        return state;
    }
}
