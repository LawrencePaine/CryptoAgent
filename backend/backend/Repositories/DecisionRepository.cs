using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CryptoAgent.Api.Repositories;

public class DecisionRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public DecisionRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LastDecision decision)
    {
        var entity = new DecisionLogEntity
        {
            TimestampUtc = decision.TimestampUtc,
            LlmAction = decision.LlmAction.ToString(),
            LlmAsset = decision.LlmAsset.ToString(),
            LlmSizeGbp = decision.LlmSizeGbp,
            LlmConfidence = decision.LlmConfidence,
            ProviderUsed = decision.ProviderUsed,
            RawModelOutput = decision.RawModelOutput,
            FinalAction = decision.FinalAction.ToString(),
            FinalAsset = decision.FinalAsset.ToString(),
            FinalSizeGbp = decision.FinalSizeGbp,
            Executed = decision.Executed,
            RationaleShort = decision.RationaleShort,
            RationaleDetailed = decision.RationaleDetailed,
            RiskReason = decision.RiskReason,
            Mode = decision.Mode
        };

        _dbContext.DecisionLogs.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<LastDecision>> GetRecentDecisionsAsync(int count)
    {
        var entities = await _dbContext.DecisionLogs
            .OrderByDescending(d => d.TimestampUtc)
            .Take(count)
            .ToListAsync();

        return entities.Select(e => new LastDecision
        {
            TimestampUtc = e.TimestampUtc,
            LlmAction = Enum.Parse<RawActionType>(e.LlmAction),
            LlmAsset = Enum.Parse<AssetType>(e.LlmAsset),
            LlmSizeGbp = e.LlmSizeGbp,
            LlmConfidence = e.LlmConfidence,
            ProviderUsed = e.ProviderUsed,
            RawModelOutput = e.RawModelOutput,
            FinalAction = Enum.Parse<RawActionType>(e.FinalAction),
            FinalAsset = Enum.Parse<AssetType>(e.FinalAsset),
            FinalSizeGbp = e.FinalSizeGbp,
            Executed = e.Executed,
            RationaleShort = e.RationaleShort,
            RationaleDetailed = e.RationaleDetailed,
            RiskReason = e.RiskReason,
            Mode = e.Mode
        }).ToList();
    }

    public Task<List<LastDecision>> GetRecentAsync(int count) => GetRecentDecisionsAsync(count);
}
