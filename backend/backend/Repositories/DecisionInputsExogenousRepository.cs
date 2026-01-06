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
            existing.ThemeStrengthJson = entity.ThemeStrengthJson;
            existing.ThemeDirectionJson = entity.ThemeDirectionJson;
            existing.ThemeConflictJson = entity.ThemeConflictJson;
            existing.AlignmentFlagsJson = entity.AlignmentFlagsJson;
            existing.MarketAlignmentJson = entity.MarketAlignmentJson;
            existing.GatingReasonJson = entity.GatingReasonJson;
            existing.AbstainModifier = entity.AbstainModifier;
            existing.ConfidenceThresholdModifier = entity.ConfidenceThresholdModifier;
            existing.PositionSizeModifier = entity.PositionSizeModifier;
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
        var themeStrength = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.ThemeStrengthJson)
            ?? new Dictionary<string, decimal>();
        if (themeStrength.Count == 0 && themeScores.Count > 0)
        {
            themeStrength = new Dictionary<string, decimal>(themeScores);
        }
        var themeDirection = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.ThemeDirectionJson)
            ?? new Dictionary<string, string>();
        var themeConflict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.ThemeConflictJson)
            ?? new Dictionary<string, decimal>();
        var alignment = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.AlignmentFlagsJson)
            ?? new Dictionary<string, string>();
        var marketAlignment = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MarketAlignmentJson)
            ?? new Dictionary<string, string>();
        var gatingReasons = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(entity.GatingReasonJson)
            ?? new Dictionary<string, List<string>>();
        var traceIds = JsonSerializer.Deserialize<List<string>>(entity.TraceIdsJson)
            ?? new List<string>();

        return new ExogenousDecisionInputsDto
        {
            TimestampUtc = entity.TimestampUtc,
            ThemeScores = themeScores,
            ThemeStrength = themeStrength,
            ThemeDirection = themeDirection,
            ThemeConflict = themeConflict,
            AlignmentFlags = alignment,
            MarketAlignment = marketAlignment,
            GatingReasons = gatingReasons,
            AbstainModifier = entity.AbstainModifier,
            ConfidenceThresholdModifier = entity.ConfidenceThresholdModifier,
            PositionSizeModifier = entity.PositionSizeModifier,
            Notes = entity.Notes,
            TraceIds = traceIds
        };
    }
}
