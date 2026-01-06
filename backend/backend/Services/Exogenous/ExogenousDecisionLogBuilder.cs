using System.Text.Json;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousDecisionLogBuilder
{
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;
    private readonly NarrativeRepository _narrativeRepository;
    private readonly ExogenousItemRepository _itemRepository;

    public ExogenousDecisionLogBuilder(
        DecisionInputsExogenousRepository decisionInputsRepository,
        NarrativeRepository narrativeRepository,
        ExogenousItemRepository itemRepository)
    {
        _decisionInputsRepository = decisionInputsRepository;
        _narrativeRepository = narrativeRepository;
        _itemRepository = itemRepository;
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

        var trace = new ExogenousDecisionTrace
        {
            AbstainModifier = latest.AbstainModifier,
            ConfidenceThresholdModifier = latest.ConfidenceThresholdModifier,
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
                Url = i.Url
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
