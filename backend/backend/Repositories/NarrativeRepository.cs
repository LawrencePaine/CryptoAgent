using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class NarrativeRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public NarrativeRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NarrativeEntity>> GetActiveByThemeAsync(string theme, DateTime windowStart)
    {
        var entities = await _dbContext.Narratives
            .Where(n => n.Theme == theme && n.LastUpdatedAt >= windowStart)
            .ToListAsync();

        return entities.OrderByDescending(n => n.StateScore).ToList();
    }

    public async Task<List<NarrativeEntity>> GetActiveNarrativesAsync(DateTime windowStart)
    {
        var entities = await _dbContext.Narratives
            .Where(n => n.LastUpdatedAt >= windowStart && n.IsActive)
            .ToListAsync();

        return entities.OrderByDescending(n => n.StateScore).ToList();
    }

    public async Task AddAsync(NarrativeEntity entity)
    {
        _dbContext.Narratives.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(NarrativeEntity entity)
    {
        _dbContext.Narratives.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddNarrativeItemAsync(NarrativeItemEntity entity)
    {
        _dbContext.NarrativeItems.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<NarrativeItemEntity>> GetNarrativeItemsAsync(Guid narrativeId)
    {
        return await _dbContext.NarrativeItems
            .Where(n => n.NarrativeId == narrativeId)
            .ToListAsync();
    }

    public async Task<List<NarrativeItemEntity>> GetNarrativeItemsAsync(IEnumerable<Guid> narrativeIds)
    {
        var ids = narrativeIds.ToList();
        if (ids.Count == 0)
        {
            return new List<NarrativeItemEntity>();
        }

        return await _dbContext.NarrativeItems
            .Where(n => ids.Contains(n.NarrativeId))
            .ToListAsync();
    }

    public async Task<List<NarrativeEntity>> GetByThemeAsync(string theme)
    {
        return await _dbContext.Narratives
            .Where(n => n.Theme == theme)
            .OrderByDescending(n => n.LastUpdatedAt)
            .ToListAsync();
    }

    public async Task<List<NarrativeEntity>> GetByIdsAsync(IEnumerable<Guid> narrativeIds)
    {
        var ids = narrativeIds.ToList();
        if (ids.Count == 0)
        {
            return new List<NarrativeEntity>();
        }

        return await _dbContext.Narratives
            .Where(n => ids.Contains(n.Id))
            .ToListAsync();
    }

    public async Task ClearNarrativesAsync(string theme, DateTime windowStart)
    {
        var narratives = await _dbContext.Narratives
            .Where(n => n.Theme == theme && n.LastUpdatedAt >= windowStart)
            .ToListAsync();

        if (narratives.Count == 0)
        {
            return;
        }

        _dbContext.Narratives.RemoveRange(narratives);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearNarrativeItemsAsync(DateTime windowStart)
    {
        var items = await _dbContext.NarrativeItems
            .Where(n => n.AddedAt >= windowStart)
            .ToListAsync();

        if (items.Count == 0)
        {
            return;
        }

        _dbContext.NarrativeItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync();
    }
}
