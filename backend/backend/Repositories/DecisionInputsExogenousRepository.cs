using System.Text.Json;
using CryptoAgent.Api.Data;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;
using Microsoft.EntityFrameworkCore;

namespace CryptoAgent.Api.Repositories;

public class DecisionInputsExogenousRepository
{
    private readonly CryptoAgentDbContext _dbContext;

    public DecisionInputsExogenousRepository(CryptoAgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpsertAsync(DecisionInputsExogenousEntity entity)
    {
        var existing = await _dbContext.DecisionInputsExogenous
            .FirstOrDefaultAsync(d => d.TimestampUtc == entity.TimestampUtc);

        if (existing == null)
        {
            _dbContext.DecisionInputsExogenous.Add(entity);
        }
        else
        {
            existing.ThemeScoresJson = entity.ThemeScoresJson;
            existing.AlignmentFlagsJson = entity.AlignmentFlagsJson;
            existing.AbstainModifier = entity.AbstainModifier;
            existing.ConfidenceThresholdModifier = entity.ConfidenceThresholdModifier;
            existing.Notes = entity.Notes;
            existing.TraceIdsJson = entity.TraceIdsJson;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<DecisionInputsExogenousEntity?> GetLatestAsync()
    {
        return await _dbContext.DecisionInputsExogenous
            .OrderByDescending(d => d.TimestampUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<DecisionInputsExogenousEntity>> GetRangeAsync(DateTime fromUtc, DateTime toUtc)
    {
        return await _dbContext.DecisionInputsExogenous
            .Where(d => d.TimestampUtc >= fromUtc && d.TimestampUtc <= toUtc)
            .OrderBy(d => d.TimestampUtc)
            .ToListAsync();
    }

    public static ExogenousDecisionInputsDto ToDto(DecisionInputsExogenousEntity entity)
    {
        var themeScores = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.ThemeScoresJson)
            ?? new Dictionary<string, decimal>();
        var alignment = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.AlignmentFlagsJson)
            ?? new Dictionary<string, string>();
        var traceIds = JsonSerializer.Deserialize<List<string>>(entity.TraceIdsJson)
            ?? new List<string>();

        return new ExogenousDecisionInputsDto
        {
            TimestampUtc = entity.TimestampUtc,
            ThemeScores = themeScores,
            AlignmentFlags = alignment,
            AbstainModifier = entity.AbstainModifier,
            ConfidenceThresholdModifier = entity.ConfidenceThresholdModifier,
            Notes = entity.Notes,
            TraceIds = traceIds
        };
    }
}
