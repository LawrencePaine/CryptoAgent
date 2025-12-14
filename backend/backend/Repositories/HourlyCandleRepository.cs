using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class HourlyCandleRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public HourlyCandleRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertBatchAsync(IEnumerable<HourlyCandleEntity> candles)
    {
        foreach (var candle in candles)
        {
            var existing = await _dbContext.HourlyCandles
                .FirstOrDefaultAsync(c => c.Asset == candle.Asset && c.HourUtc == candle.HourUtc);

            if (existing == null)
            {
                _dbContext.HourlyCandles.Add(candle);
            }
            else
            {
                existing.OpenGbp = candle.OpenGbp;
                existing.HighGbp = candle.HighGbp;
                existing.LowGbp = candle.LowGbp;
                existing.CloseGbp = candle.CloseGbp;
                existing.Volume = candle.Volume;
                existing.Source = candle.Source;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<HourlyCandleEntity?> GetLatestAsync(string asset)
    {
        return await _dbContext.HourlyCandles
            .Where(c => c.Asset == asset)
            .OrderByDescending(c => c.HourUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<HourlyCandleEntity>> GetWindowAsync(string asset, DateTime fromHourUtc, DateTime toHourUtc)
    {
        return await _dbContext.HourlyCandles
            .Where(c => c.Asset == asset && c.HourUtc >= fromHourUtc && c.HourUtc <= toHourUtc)
            .OrderBy(c => c.HourUtc)
            .ToListAsync();
    }
}
