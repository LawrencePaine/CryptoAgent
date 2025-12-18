using CryptoAgent.Api.Models;

namespace CryptoAgent.Api.Services;

public class PortfolioValuationService
{
    public PortfolioValuation Calculate(Portfolio portfolio, MarketSnapshot market)
    {
        var btcValue = portfolio.BtcAmount * market.BtcPriceGbp;
        var ethValue = portfolio.EthAmount * market.EthPriceGbp;
        var total = portfolio.CashGbp + portfolio.VaultGbp + btcValue + ethValue;

        var btcCostBasis = Math.Max(0, portfolio.BtcCostBasisGbp);
        var ethCostBasis = Math.Max(0, portfolio.EthCostBasisGbp);
        portfolio.BtcCostBasisGbp = btcCostBasis;
        portfolio.EthCostBasisGbp = ethCostBasis;

        portfolio.BtcValueGbp = btcValue;
        portfolio.EthValueGbp = ethValue;
        portfolio.TotalValueGbp = total;
        portfolio.BtcAllocationPct = total > 0 ? btcValue / total : 0;
        portfolio.EthAllocationPct = total > 0 ? ethValue / total : 0;
        portfolio.CashAllocationPct = total > 0 ? portfolio.CashGbp / total : 0;

        var btcUnrealised = btcValue - btcCostBasis;
        var ethUnrealised = ethValue - ethCostBasis;

        return new PortfolioValuation
        {
            BtcValueGbp = btcValue,
            EthValueGbp = ethValue,
            TotalValueGbp = total,
            BtcUnrealisedPnlGbp = btcUnrealised,
            EthUnrealisedPnlGbp = ethUnrealised,
            BtcUnrealisedPnlPct = btcCostBasis > 0 ? btcUnrealised / btcCostBasis : 0,
            EthUnrealisedPnlPct = ethCostBasis > 0 ? ethUnrealised / ethCostBasis : 0,
            BtcAllocationPct = total > 0 ? btcValue / total : 0,
            EthAllocationPct = total > 0 ? ethValue / total : 0,
            CashAllocationPct = total > 0 ? portfolio.CashGbp / total : 0,
            VaultAllocationPct = total > 0 ? portfolio.VaultGbp / total : 0
        };
    }
}
