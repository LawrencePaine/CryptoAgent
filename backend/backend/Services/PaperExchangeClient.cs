using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services;

public class PaperExchangeClient : IExchangeClient
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly FeeConfig _feeConfig;
    private const decimal SlippagePct = 0.001m;

    public PaperExchangeClient(PortfolioRepository portfolioRepository, MarketDataService marketDataService, FeeConfig feeConfig)
    {
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _feeConfig = feeConfig;
    }

    public async Task<decimal> GetBalanceAsync(string symbol)
    {
        var portfolio = await _portfolioRepository.GetAsync();
        return symbol.ToUpperInvariant() switch
        {
            "GBP" => portfolio.CashGbp,
            "BTC" => portfolio.BtcAmount,
            "ETH" => portfolio.EthAmount,
            _ => 0m
        };
    }

    public async Task<ExchangeTrade> PlaceMarketOrderAsync(string symbol, decimal sizeGbp, OrderSide side)
    {
        var portfolio = await _portfolioRepository.GetAsync();
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

        await _portfolioRepository.SaveAsync(portfolio);

        var trade = new Trade
        {
            TimestampUtc = DateTime.UtcNow,
            Asset = symbol.ToUpperInvariant() == "BTC" ? AssetType.Btc : AssetType.Eth,
            Action = side == OrderSide.Buy ? RawActionType.Buy : RawActionType.Sell,
            AssetAmount = assetAmount,
            SizeGbp = sizeGbp,
            PriceGbp = effectivePrice,
            FeeGbp = feeGbp,
            Mode = "PAPER"
        };

        await _portfolioRepository.LogTradeAsync(trade);

        return new ExchangeTrade
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
    }

    public async Task<IReadOnlyList<ExchangeTrade>> GetRecentTradesAsync(int count)
    {
        var trades = await _portfolioRepository.GetRecentTradesAsync(count);
        return trades.Select(t => new ExchangeTrade
        {
            TimestampUtc = t.TimestampUtc,
            Asset = t.Asset == AssetType.Btc ? "BTC" : "ETH",
            Side = t.Action == RawActionType.Buy ? OrderSide.Buy : OrderSide.Sell,
            AssetAmount = t.AssetAmount,
            SizeGbp = t.SizeGbp,
            PriceGbp = t.PriceGbp,
            FeeGbp = t.FeeGbp,
            Mode = t.Mode
        }).ToList();
    }
}
