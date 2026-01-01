using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class ExogenousItemRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public ExogenousItemRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DateTime?> GetLatestPublishedAtAsync(string sourceId)
    {
        return await _dbContext.ExogenousItems
            .Where(i => i.SourceId == sourceId)
            .OrderByDescending(i => i.PublishedAt)
            .Select(i => (DateTime?)i.PublishedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> AddNewItemsAsync(IEnumerable<ExogenousItemEntity> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return 0;
        }

        var urls = itemList.Select(i => i.Url).ToList();
        var hashes = itemList.Select(i => i.ContentHash).ToList();

        var existing = await _dbContext.ExogenousItems
            .Where(i => urls.Contains(i.Url) || hashes.Contains(i.ContentHash))
            .Select(i => new { i.Url, i.ContentHash })
            .ToListAsync();

        var existingUrls = existing.Select(e => e.Url).ToHashSet();
        var existingHashes = existing.Select(e => e.ContentHash).ToHashSet();

        var toAdd = itemList
            .Where(i => !existingUrls.Contains(i.Url) && !existingHashes.Contains(i.ContentHash))
            .ToList();

        if (toAdd.Count == 0)
        {
            return 0;
        }

        _dbContext.ExogenousItems.AddRange(toAdd);
        await _dbContext.SaveChangesAsync();
        return toAdd.Count;
    }

    public async Task<List<ExogenousItemEntity>> GetByStatusAsync(string status, int limit)
    {
        return await _dbContext.ExogenousItems
            .Where(i => i.Status == status)
            .OrderBy(i => i.PublishedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ExogenousItemEntity>> GetClassifiedUnassignedAsync(int limit)
    {
        return await _dbContext.ExogenousItems
            .Where(i => i.Status == "CLASSIFIED")
            .Where(i => !_dbContext.NarrativeItems.Any(n => n.ItemId == i.Id))
            .OrderBy(i => i.PublishedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(Guid itemId, string status, string? error)
    {
        var entity = await _dbContext.ExogenousItems.FirstOrDefaultAsync(i => i.Id == itemId);
        if (entity == null)
        {
            return;
        }

        entity.Status = status;
        entity.Error = error;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ExogenousItemEntity>> GetItemsByIdsAsync(IEnumerable<Guid> itemIds)
    {
        var ids = itemIds.ToList();
        if (ids.Count == 0)
        {
            return new List<ExogenousItemEntity>();
        }

        return await _dbContext.ExogenousItems
            .Where(i => ids.Contains(i.Id))
            .ToListAsync();
    }
}
