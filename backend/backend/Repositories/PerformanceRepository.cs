using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class PerformanceRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public PerformanceRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PerformanceSnapshot>> GetAllAsync()
    {
        var snapshots = await _dbContext.PerformanceSnapshots
            .OrderBy(p => p.DateUtc)
            .ToListAsync();

        return snapshots.Select(MapToDomain).ToList();
    }

    public async Task AppendAsync(PerformanceSnapshot snapshot)
    {
        var entity = new PerformanceSnapshotEntity
        {
            DateUtc = snapshot.DateUtc,
            PortfolioValueGbp = snapshot.PortfolioValueGbp,
            VaultGbp = snapshot.VaultGbp,
            NetDepositsGbp = snapshot.NetDepositsGbp,
            CumulatedAiCostGbp = snapshot.CumulatedAiCostGbp,
            CumulatedFeesGbp = snapshot.CumulatedFeesGbp
        };

        _dbContext.PerformanceSnapshots.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    private static PerformanceSnapshot MapToDomain(PerformanceSnapshotEntity entity)
    {
        return new PerformanceSnapshot
        {
            DateUtc = entity.DateUtc,
            PortfolioValueGbp = entity.PortfolioValueGbp,
            VaultGbp = entity.VaultGbp,
            NetDepositsGbp = entity.NetDepositsGbp,
            CumulatedAiCostGbp = entity.CumulatedAiCostGbp,
            CumulatedFeesGbp = entity.CumulatedFeesGbp
        };
    }
}
