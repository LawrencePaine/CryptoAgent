using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class HourlyFeatureRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public HourlyFeatureRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(HourlyFeatureEntity feature)
    {
        var existing = await _dbContext.HourlyFeatures
            .FirstOrDefaultAsync(f => f.Asset == feature.Asset && f.HourUtc == feature.HourUtc);

        if (existing == null)
        {
            _dbContext.HourlyFeatures.Add(feature);
        }
        else
        {
            existing.Return1h = feature.Return1h;
            existing.Return24h = feature.Return24h;
            existing.Return7d = feature.Return7d;
            existing.Vol24h = feature.Vol24h;
            existing.Vol72h = feature.Vol72h;
            existing.Sma24 = feature.Sma24;
            existing.Sma168 = feature.Sma168;
            existing.TrendStrength = feature.TrendStrength;
            existing.Drawdown7d = feature.Drawdown7d;
            existing.MomentumScore = feature.MomentumScore;
            existing.IsComplete = feature.IsComplete;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<HourlyFeatureEntity?> GetLatestAsync(string asset)
    {
        return await _dbContext.HourlyFeatures
            .Where(f => f.Asset == asset)
            .OrderByDescending(f => f.HourUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<HourlyFeatureEntity>> GetRecentAsync(string asset, int count)
    {
        return await _dbContext.HourlyFeatures
            .Where(f => f.Asset == asset)
            .OrderByDescending(f => f.HourUtc)
            .Take(count)
            .ToListAsync();
    }
}
