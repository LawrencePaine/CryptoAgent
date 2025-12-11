using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services;

public class CryptoComExchangeClient : IExchangeClient
{
    private readonly PortfolioRepository _portfolioRepository;
    private readonly IConfiguration _configuration;

    public CryptoComExchangeClient(PortfolioRepository portfolioRepository, IConfiguration configuration)
    {
        _portfolioRepository = portfolioRepository;
        _configuration = configuration;
    }

    public Task<decimal> GetBalanceAsync(string symbol)
    {
        throw new NotImplementedException("Crypto.com live integration will be implemented in Phase 2.");
    }

    public Task<ExchangeTrade> PlaceMarketOrderAsync(string symbol, decimal sizeGbp, OrderSide side)
    {
        throw new NotImplementedException("Crypto.com live integration will be implemented in Phase 2.");
    }

    public Task<IReadOnlyList<ExchangeTrade>> GetRecentTradesAsync(int count)
    {
        throw new NotImplementedException("Crypto.com live integration will be implemented in Phase 2.");
    }
}
