using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class StrategySignalRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public StrategySignalRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertBatchAsync(IEnumerable<StrategySignalEntity> signals)
    {
        foreach (var signal in signals)
        {
            var existing = await _dbContext.StrategySignals
                .FirstOrDefaultAsync(s => s.Asset == signal.Asset && s.HourUtc == signal.HourUtc && s.StrategyName == signal.StrategyName);

            if (existing == null)
            {
                _dbContext.StrategySignals.Add(signal);
            }
            else
            {
                existing.SignalScore = signal.SignalScore;
                existing.SuggestedAction = signal.SuggestedAction;
                existing.SuggestedSizeGbp = signal.SuggestedSizeGbp;
                existing.Reason = signal.Reason;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<StrategySignalEntity>> GetLatestSignalsAsync(string asset, DateTime hourUtc)
    {
        return await _dbContext.StrategySignals
            .Where(s => s.Asset == asset && s.HourUtc == hourUtc)
            .OrderByDescending(s => Math.Abs(s.SignalScore))
            .ToListAsync();
    }

    public async Task<List<StrategySignalEntity>> GetRecentSignalsAsync(string asset, int count)
    {
        return await _dbContext.StrategySignals
            .Where(s => s.Asset == asset)
            .OrderByDescending(s => s.HourUtc)
            .ThenByDescending(s => Math.Abs(s.SignalScore))
            .Take(count)
            .ToListAsync();
    }
}
