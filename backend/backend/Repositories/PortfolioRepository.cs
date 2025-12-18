using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class PortfolioRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public PortfolioRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Portfolio> GetAsync()
    {
        var entity = await _dbContext.Portfolios.FirstOrDefaultAsync(p => p.Id == 1);
        if (entity == null)
        {
            entity = new PortfolioEntity
            {
                Id = 1,
                CashGbp = 50m,
                BtcAmount = 0,
                EthAmount = 0,
                BtcCostBasisGbp = 0,
                EthCostBasisGbp = 0,
                VaultGbp = 0,
                HighWatermarkGbp = 0
            };
            _dbContext.Portfolios.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        return MapToDomain(entity);
    }

    public async Task SaveAsync(Portfolio portfolio)
    {
        var entity = await _dbContext.Portfolios.FirstOrDefaultAsync(p => p.Id == 1);
        if (entity == null)
        {
            entity = new PortfolioEntity { Id = 1 };
            _dbContext.Portfolios.Add(entity);
        }

        entity.CashGbp = portfolio.CashGbp;
        entity.BtcAmount = portfolio.BtcAmount;
        entity.EthAmount = portfolio.EthAmount;
        entity.BtcCostBasisGbp = portfolio.BtcCostBasisGbp;
        entity.EthCostBasisGbp = portfolio.EthCostBasisGbp;
        entity.VaultGbp = portfolio.VaultGbp;
        entity.HighWatermarkGbp = portfolio.HighWatermarkGbp;

        await _dbContext.SaveChangesAsync();
    }

    public async Task LogTradeAsync(Trade trade)
    {
        var entity = new TradeEntity
        {
            TimestampUtc = trade.TimestampUtc,
            Asset = trade.Asset == AssetType.Btc ? "BTC" : "ETH",
            Action = trade.Action == RawActionType.Buy ? "BUY" : "SELL",
            AssetAmount = trade.AssetAmount,
            SizeGbp = trade.SizeGbp,
            PriceGbp = trade.PriceGbp,
            FeeGbp = trade.FeeGbp,
            Mode = trade.Mode
        };

        _dbContext.Trades.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Trade>> GetRecentTradesAsync(int count)
    {
        var trades = await _dbContext.Trades
            .OrderByDescending(t => t.TimestampUtc)
            .Take(count)
            .ToListAsync();

        return trades.Select(MapToDomain).ToList();
    }

    public async Task<int> CountTradesTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbContext.Trades.CountAsync(t => t.TimestampUtc.Date == today);
    }

    public async Task<decimal> GetTotalFeesAsync()
    {
        var fees = await _dbContext.Trades.Select(t => t.FeeGbp).ToListAsync();
        return fees.Sum();
    }

    private static Portfolio MapToDomain(PortfolioEntity entity)
    {
        return new Portfolio
        {
            CashGbp = entity.CashGbp,
            BtcAmount = entity.BtcAmount,
            EthAmount = entity.EthAmount,
            BtcCostBasisGbp = entity.BtcCostBasisGbp,
            EthCostBasisGbp = entity.EthCostBasisGbp,
            VaultGbp = entity.VaultGbp,
            HighWatermarkGbp = entity.HighWatermarkGbp
        };
    }

    private static Trade MapToDomain(TradeEntity entity)
    {
        return new Trade
        {
            TimestampUtc = entity.TimestampUtc,
            Asset = entity.Asset.ToUpperInvariant() == "BTC" ? AssetType.Btc : AssetType.Eth,
            Action = entity.Action.ToUpperInvariant() == "BUY" ? RawActionType.Buy : RawActionType.Sell,
            AssetAmount = entity.AssetAmount,
            SizeGbp = entity.SizeGbp,
            PriceGbp = entity.PriceGbp,
            FeeGbp = entity.FeeGbp,
            Mode = entity.Mode
        };
    }
}
