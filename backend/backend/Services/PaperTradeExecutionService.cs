using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services;

public class PaperTradeExecutionService
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly FeeConfig _feeConfig;
    private const decimal SlippagePct = 0.001m;

    public PaperTradeExecutionService(
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        FeeConfig feeConfig)
    {
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _feeConfig = feeConfig;
    }

    public async Task<(Portfolio Portfolio, Trade Trade, ExchangeTrade ExchangeTrade, MarketSnapshot Market)> ExecuteAsync(
        PortfolioBook book,
        string symbol,
        decimal sizeGbp,
        OrderSide side,
        CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetAsync(book);
        var market = await _marketDataService.GetSnapshotAsync();
        var price = symbol.ToUpperInvariant() switch
        {
            "BTC" => market.BtcPriceGbp,
            "ETH" => market.EthPriceGbp,
            _ => 0m
        };

        if (price <= 0)
        {
            throw new InvalidOperationException($"Invalid price for {symbol}");
        }

        var effectivePrice = side == OrderSide.Buy
            ? price * (1 + SlippagePct)
            : price * (1 - SlippagePct);

        var feeGbp = sizeGbp * _feeConfig.TakerPct;
        var assetAmount = sizeGbp / effectivePrice;

        if (side == OrderSide.Buy)
        {
            var totalCost = sizeGbp + feeGbp;
            if (totalCost > portfolio.CashGbp)
            {
                throw new InvalidOperationException("Insufficient cash for buy order");
            }

            portfolio.CashGbp -= totalCost;
            if (symbol.ToUpperInvariant() == "BTC")
            {
                portfolio.BtcAmount += assetAmount;
                portfolio.BtcCostBasisGbp += totalCost;
            }
            if (symbol.ToUpperInvariant() == "ETH")
            {
                portfolio.EthAmount += assetAmount;
                portfolio.EthCostBasisGbp += totalCost;
            }
        }
        else
        {
            if (symbol.ToUpperInvariant() == "BTC")
            {
                var holdingsBefore = portfolio.BtcAmount;
                assetAmount = Math.Min(assetAmount, holdingsBefore);
                portfolio.BtcAmount -= assetAmount;
                if (holdingsBefore > 0)
                {
                    var proportionSold = assetAmount / holdingsBefore;
                    portfolio.BtcCostBasisGbp -= portfolio.BtcCostBasisGbp * proportionSold;
                    portfolio.BtcCostBasisGbp = Math.Max(0, portfolio.BtcCostBasisGbp);
                }
            }
            else if (symbol.ToUpperInvariant() == "ETH")
            {
                var holdingsBefore = portfolio.EthAmount;
                assetAmount = Math.Min(assetAmount, holdingsBefore);
                portfolio.EthAmount -= assetAmount;
                if (holdingsBefore > 0)
                {
                    var proportionSold = assetAmount / holdingsBefore;
                    portfolio.EthCostBasisGbp -= portfolio.EthCostBasisGbp * proportionSold;
                    portfolio.EthCostBasisGbp = Math.Max(0, portfolio.EthCostBasisGbp);
                }
            }

            var adjustedSize = assetAmount * effectivePrice;
            feeGbp = adjustedSize * _feeConfig.TakerPct;
            portfolio.CashGbp += adjustedSize - feeGbp;
            sizeGbp = adjustedSize;
        }

        await _portfolioRepository.SaveAsync(portfolio, book);

        var trade = new Trade
        {
            TimestampUtc = DateTime.UtcNow,
            Asset = symbol.ToUpperInvariant() == "BTC" ? AssetType.Btc : AssetType.Eth,
            Action = side == OrderSide.Buy ? RawActionType.Buy : RawActionType.Sell,
            AssetAmount = assetAmount,
            SizeGbp = sizeGbp,
            PriceGbp = effectivePrice,
            FeeGbp = feeGbp,
            Mode = "PAPER",
            Book = book
        };

        await _portfolioRepository.LogTradeAsync(trade);

        var exchangeTrade = new ExchangeTrade
        {
            TimestampUtc = trade.TimestampUtc,
            Asset = trade.Asset == AssetType.Btc ? "BTC" : "ETH",
            Side = side,
            AssetAmount = trade.AssetAmount,
            SizeGbp = trade.SizeGbp,
            PriceGbp = trade.PriceGbp,
            FeeGbp = trade.FeeGbp,
            Mode = trade.Mode
        };

        return (portfolio, trade, exchangeTrade, market);
    }
}
