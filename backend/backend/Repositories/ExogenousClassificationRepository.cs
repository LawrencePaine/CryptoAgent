using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class ExogenousClassificationRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public ExogenousClassificationRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ExogenousClassificationEntity entity)
    {
        _dbContext.ExogenousClassifications.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public Task<ExogenousClassificationEntity?> GetByItemIdAsync(Guid itemId)
    {
        return _dbContext.ExogenousClassifications
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(c => c.ItemId == itemId);
    }

    public async Task<List<ExogenousClassificationEntity>> GetByItemIdsAsync(IEnumerable<Guid> itemIds)
    {
        var ids = itemIds.ToList();
        if (ids.Count == 0)
        {
            return new List<ExogenousClassificationEntity>();
        }

        return await _dbContext.ExogenousClassifications
            .Where(c => ids.Contains(c.ItemId))
            .ToListAsync();
    }
}
