namespace CryptoAgent.Api.Services;

public enum OrderSide { Buy, Sell }

public class ExchangeTrade
{
    public DateTime TimestampUtc { get; set; }
    public string Asset { get; set; } = string.Empty;
    public OrderSide Side { get; set; }
    public decimal AssetAmount { get; set; }
    public decimal SizeGbp { get; set; }
    public decimal PriceGbp { get; set; }
    public decimal FeeGbp { get; set; }
    public string Mode { get; set; } = "PAPER";
}

public interface IExchangeClient
{
    Task<decimal> GetBalanceAsync(string symbol);
    Task<ExchangeTrade> PlaceMarketOrderAsync(string symbol, decimal sizeGbp, OrderSide side);
    Task<IReadOnlyList<ExchangeTrade>> GetRecentTradesAsync(int count);
}
