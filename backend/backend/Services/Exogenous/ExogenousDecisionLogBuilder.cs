using System.Text.Json;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousDecisionLogBuilder
{
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;
    private readonly NarrativeRepository _narrativeRepository;
    private readonly ExogenousItemRepository _itemRepository;
    private readonly ExogenousClassificationRepository _classificationRepository;

    public ExogenousDecisionLogBuilder(
        DecisionInputsExogenousRepository decisionInputsRepository,
        NarrativeRepository narrativeRepository,
        ExogenousItemRepository itemRepository,
        ExogenousClassificationRepository classificationRepository)
    {
        _decisionInputsRepository = decisionInputsRepository;
        _narrativeRepository = narrativeRepository;
        _itemRepository = itemRepository;
        _classificationRepository = classificationRepository;
    }

    public async Task<(string traceJson, string summary)> BuildLatestAsync()
    {
        var latest = await _decisionInputsRepository.GetLatestAsync();
        if (latest == null)
        {
            return ("{}", string.Empty);
        }

        var traceIds = JsonSerializer.Deserialize<List<string>>(latest.TraceIdsJson) ?? new List<string>();
        var narrativeIds = new List<Guid>();
        var itemIds = new List<Guid>();

        foreach (var traceId in traceIds)
        {
            if (traceId.StartsWith("narrative:", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(traceId["narrative:".Length..], out var narrativeId))
            {
                narrativeIds.Add(narrativeId);
            }
            else if (traceId.StartsWith("item:", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(traceId["item:".Length..], out var itemId))
            {
                itemIds.Add(itemId);
            }
        }

        var narratives = await _narrativeRepository.GetByIdsAsync(narrativeIds);
        var items = await _itemRepository.GetItemsByIdsAsync(itemIds);
        var classifications = await _classificationRepository.GetByItemIdsAsync(itemIds);
        var classificationByItem = classifications.ToDictionary(c => c.ItemId, c => c);
        var itemContributionById = new Dictionary<Guid, ExogenousItemContribution>();

        foreach (var item in items)
        {
            if (!classificationByItem.TryGetValue(item.Id, out var classification))
            {
                continue;
            }

            var contribution = ExogenousScoring.ComputeContribution(item, classification, Guid.Empty, latest.TimestampUtc);
            if (contribution == null)
            {
                continue;
            }

            itemContributionById[item.Id] = contribution;
        }

        var themeStrength = JsonSerializer.Deserialize<Dictionary<string, decimal>>(latest.ThemeStrengthJson)
            ?? new Dictionary<string, decimal>();
        var themeDirection = JsonSerializer.Deserialize<Dictionary<string, string>>(latest.ThemeDirectionJson)
            ?? new Dictionary<string, string>();
        var themeConflict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(latest.ThemeConflictJson)
            ?? new Dictionary<string, decimal>();
        var marketAlignment = JsonSerializer.Deserialize<Dictionary<string, string>>(latest.MarketAlignmentJson)
            ?? new Dictionary<string, string>();
        var gatingReasons = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(latest.GatingReasonJson)
            ?? new Dictionary<string, List<string>>();

        var trace = new ExogenousDecisionTrace
        {
            ThemeStrength = themeStrength,
            ThemeDirection = themeDirection,
            ThemeConflict = themeConflict,
            MarketAlignment = marketAlignment,
            GatingReasons = gatingReasons,
            AbstainModifier = latest.AbstainModifier,
            ConfidenceThresholdModifier = latest.ConfidenceThresholdModifier,
            PositionSizeModifier = latest.PositionSizeModifier,
            WhyBullets = SplitNotes(latest.Notes),
            Narratives = narratives.Select(n => new ExogenousNarrativeTrace
            {
                Id = n.Id,
                Label = n.Label,
                Theme = Enum.TryParse<ExogenousTheme>(n.Theme, true, out var theme) ? theme : ExogenousTheme.NONE,
                StateScore = n.StateScore,
                DirectionalBias = Enum.TryParse<ExogenousDirectionalBias>(n.DirectionalBias, true, out var bias) ? bias : ExogenousDirectionalBias.NEUTRAL,
                Horizon = Enum.TryParse<ExogenousImpactHorizon>(n.Horizon, true, out var horizon) ? horizon : ExogenousImpactHorizon.NOISE
            }).ToList(),
            Items = items.Select(i => new ExogenousItemTrace
            {
                Id = i.Id,
                Title = i.Title,
                SourceId = i.SourceId,
                PublishedAt = i.PublishedAt,
                Url = i.Url,
                Contribution = itemContributionById.TryGetValue(i.Id, out var contribution) ? contribution.Contribution : 0m,
                SourceCredibilityWeight = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.SourceWeight : 0m,
                ConfidenceScore = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.ConfidenceWeight : 0m,
                HorizonWeight = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.HorizonWeight : 0m,
                TimeDecay = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.TimeDecay : 0m,
                DirectionalBias = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.DirectionalBias : ExogenousDirectionalBias.NEUTRAL,
                ImpactHorizon = itemContributionById.TryGetValue(i.Id, out contribution) ? contribution.ImpactHorizon : ExogenousImpactHorizon.NOISE
            }).ToList()
        };

        var traceJson = JsonSerializer.Serialize(trace, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var summary = trace.WhyBullets.Count > 0
            ? string.Join(" ", trace.WhyBullets)
            : latest.Notes;

        return (traceJson, summary);
    }

    private static List<string> SplitNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return new List<string>();
        }

        return notes.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.TrimStart('-', ' ').Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
    }
}
