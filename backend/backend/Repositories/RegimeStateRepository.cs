using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class RegimeStateRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public RegimeStateRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(RegimeStateEntity entity)
    {
        var existing = await _dbContext.RegimeStates
            .FirstOrDefaultAsync(r => r.Asset == entity.Asset && r.HourUtc == entity.HourUtc);

        if (existing == null)
        {
            _dbContext.RegimeStates.Add(entity);
        }
        else
        {
            existing.Regime = entity.Regime;
            existing.Reason = entity.Reason;
            existing.Confidence = entity.Confidence;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<RegimeStateEntity?> GetLatestAsync(string asset)
    {
        return await _dbContext.RegimeStates
            .Where(r => r.Asset == asset)
            .OrderByDescending(r => r.HourUtc)
            .FirstOrDefaultAsync();
    }
}
