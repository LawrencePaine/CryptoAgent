using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services;

public class PaperExchangeClient : IExchangeClient
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly PaperTradeExecutionService _tradeExecutionService;

    public PaperExchangeClient(PortfolioRepository portfolioRepository, PaperTradeExecutionService tradeExecutionService)
    {
        _portfolioRepository = portfolioRepository;
        _tradeExecutionService = tradeExecutionService;
    }

    public async Task<decimal> GetBalanceAsync(string symbol)
    {
        var portfolio = await _portfolioRepository.GetAsync(PortfolioBook.Agent);
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
        var result = await _tradeExecutionService.ExecuteAsync(PortfolioBook.Agent, symbol, sizeGbp, side);
        return result.ExchangeTrade;
    }

    public async Task<IReadOnlyList<ExchangeTrade>> GetRecentTradesAsync(int count)
    {
        var trades = await _portfolioRepository.GetRecentTradesAsync(count, PortfolioBook.Agent);
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
