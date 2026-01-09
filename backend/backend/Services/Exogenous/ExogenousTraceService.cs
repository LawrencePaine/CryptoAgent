using System.Globalization;
using System.Text.Json;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousTraceService : IExogenousTraceService
{
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;
    private readonly NarrativeRepository _narrativeRepository;
    private readonly ExogenousItemRepository _itemRepository;
    private readonly ExogenousClassificationRepository _classificationRepository;
    private readonly ILogger<ExogenousTraceService> _logger;

    public ExogenousTraceService(
        DecisionInputsExogenousRepository decisionInputsRepository,
        NarrativeRepository narrativeRepository,
        ExogenousItemRepository itemRepository,
        ExogenousClassificationRepository classificationRepository,
        ILogger<ExogenousTraceService> logger)
    {
        _decisionInputsRepository = decisionInputsRepository;
        _narrativeRepository = narrativeRepository;
        _itemRepository = itemRepository;
        _classificationRepository = classificationRepository;
        _logger = logger;
    }

    public async Task<ExogenousDecisionTraceDto?> GetTraceAsync(DateTime tickUtc, int topNarratives = 5, int topItems = 10)
    {
        var entity = await _decisionInputsRepository.GetByTimestampAsync(tickUtc);
        if (entity == null)
        {
            return null;
        }

        var themeStrength = DeserializeOrDefault(entity.ThemeStrengthJson, new Dictionary<string, decimal>());
        var themeDirection = DeserializeOrDefault(entity.ThemeDirectionJson, new Dictionary<string, string>());
        var themeConflict = DeserializeOrDefault(entity.ThemeConflictJson, new Dictionary<string, decimal>());
        var marketAlignment = DeserializeOrDefault(entity.MarketAlignmentJson, new Dictionary<string, string>());
        var gatingReasons = DeserializeOrDefault(entity.GatingReasonJson, new Dictionary<string, List<string>>());

        var themes = BuildThemes(themeStrength, themeDirection, themeConflict);
        var whyReasons = BuildWhyReasons(gatingReasons, entity.Notes, themeStrength, themeDirection, themeConflict, marketAlignment);

        var trace = new ExogenousDecisionTraceDto
        {
            TickUtc = entity.TimestampUtc,
            Themes = themes,
            MarketAlignment = marketAlignment,
            Modifiers = new ModifiersDto
            {
                AbstainModifier = (double)entity.AbstainModifier,
                ConfidenceThresholdModifier = (double)entity.ConfidenceThresholdModifier,
                PositionSizeModifier = entity.PositionSizeModifier == 0m ? null : (double)entity.PositionSizeModifier
            },
            Summary = BuildSummary(themes, marketAlignment),
            GatingReasons = whyReasons.Select(r => r.Reason).ToList(),
            Why = whyReasons
        };

        if (!TryDeserialize(entity.TraceIdsJson, out List<string> traceIds))
        {
            _logger.LogError("Failed to parse exogenous trace ids for tick {TickUtc}", entity.TimestampUtc);
            return trace;
        }

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

        if (narrativeIds.Count == 0 && itemIds.Count == 0)
        {
            return trace;
        }

        var narratives = await _narrativeRepository.GetByIdsAsync(narrativeIds);
        var narrativeItems = await _narrativeRepository.GetNarrativeItemsAsync(narrativeIds);
        var narrativeItemCounts = narrativeItems
            .GroupBy(i => i.NarrativeId)
            .ToDictionary(g => g.Key, g => g.Count());

        trace.TopNarratives = narratives
            .OrderByDescending(n => n.StateScore)
            .Take(Math.Max(0, topNarratives))
            .Select(n => new NarrativeDto
            {
                Id = n.Id.ToString(),
                Theme = n.Theme,
                Label = n.Label,
                StateScore = (int)Math.Round(n.StateScore),
                Direction = n.DirectionalBias,
                Horizon = n.Horizon,
                LastUpdatedUtc = n.LastUpdatedAt,
                ItemCount = narrativeItemCounts.TryGetValue(n.Id, out var count) ? count : 0
            })
            .ToList();

        var items = await _itemRepository.GetItemsByIdsAsync(itemIds);
        var classifications = await _classificationRepository.GetByItemIdsAsync(itemIds);
        var classificationByItem = classifications.ToDictionary(c => c.ItemId, c => c);
        var itemContributions = new Dictionary<Guid, ExogenousItemContribution>();

        foreach (var item in items)
        {
            if (!classificationByItem.TryGetValue(item.Id, out var classification))
            {
                continue;
            }

            var contribution = ExogenousScoring.ComputeContribution(item, classification, Guid.Empty, entity.TimestampUtc);
            if (contribution == null)
            {
                continue;
            }

            itemContributions[item.Id] = contribution;
        }

        trace.TopItems = items
            .Select(i => MapItem(i, classificationByItem, itemContributions))
            .OrderByDescending(i => Math.Abs(i.ContributionWeight ?? 0))
            .ThenByDescending(i => i.PublishedAtUtc)
            .Take(Math.Max(0, topItems))
            .ToList();

        return trace;
    }

    private static ItemDto MapItem(
        ExogenousItemEntity item,
        IReadOnlyDictionary<Guid, ExogenousClassificationEntity> classificationByItem,
        IReadOnlyDictionary<Guid, ExogenousItemContribution> itemContributions)
    {
        classificationByItem.TryGetValue(item.Id, out var classification);
        itemContributions.TryGetValue(item.Id, out var contribution);

        var theme = classification?.ThemeRelevance ?? "NONE";
        var horizon = classification?.ImpactHorizon ?? "NOISE";
        var bias = classification?.DirectionalBias ?? "NEUTRAL";
        var confidence = classification?.ConfidenceScore ?? 0m;

        return new ItemDto
        {
            Id = item.Id.ToString(),
            PublishedAtUtc = item.PublishedAt,
            SourceId = item.SourceId,
            Title = item.Title,
            Url = item.Url,
            Theme = theme,
            ImpactHorizon = horizon,
            DirectionalBias = bias,
            ConfidenceScore = (double)confidence,
            ContributionWeight = contribution == null ? null : (double)contribution.Contribution
        };
    }

    private static List<ThemeSummaryDto> BuildThemes(
        Dictionary<string, decimal> strength,
        Dictionary<string, string> direction,
        Dictionary<string, decimal> conflict)
    {
        var keys = strength.Keys
            .Concat(direction.Keys)
            .Concat(conflict.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return keys.Select(key => new ThemeSummaryDto
            {
                Theme = key,
                Strength = (int)Math.Clamp(Math.Round(strength.GetValueOrDefault(key, 0m)), 0, 100),
                Direction = direction.GetValueOrDefault(key, "NEUTRAL"),
                Conflict = (double)Math.Clamp(conflict.GetValueOrDefault(key, 0m), 0m, 1m)
            })
            .OrderByDescending(t => t.Strength)
            .ToList();
    }

    private static List<WhyReasonDto> BuildWhyReasons(
        Dictionary<string, List<string>> gatingReasons,
        string notes,
        Dictionary<string, decimal> strength,
        Dictionary<string, string> direction,
        Dictionary<string, decimal> conflict,
        Dictionary<string, string> marketAlignment)
    {
        var reasons = new List<WhyReasonDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddReason(string reason, string? tag)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            var trimmed = reason.Trim();
            if (!seen.Add(trimmed))
            {
                return;
            }

            reasons.Add(new WhyReasonDto
            {
                Reason = trimmed,
                Tag = string.IsNullOrWhiteSpace(tag) ? null : NormalizeTag(tag)
            });
        }

        foreach (var (tag, entries) in gatingReasons)
        {
            foreach (var entry in entries)
            {
                AddReason(entry, tag);
            }
        }

        if (reasons.Count == 0)
        {
            foreach (var note in SplitNotes(notes))
            {
                AddReason(note, "Notes");
            }
        }

        if (reasons.Count == 0)
        {
            foreach (var (asset, alignment) in marketAlignment)
            {
                AddReason($"Market alignment {asset}: {alignment}.", "Market");
            }

            foreach (var metric in strength)
            {
                var dir = direction.GetValueOrDefault(metric.Key, "NEUTRAL");
                var conf = conflict.GetValueOrDefault(metric.Key, 0m);
                AddReason($"{metric.Key} strength {metric.Value:F0}, direction {dir}, conflict {conf:F2}.", "Theme");
            }
        }

        return reasons.Take(6).ToList();
    }

    private static string BuildSummary(IReadOnlyCollection<ThemeSummaryDto> themes, IReadOnlyDictionary<string, string> marketAlignment)
    {
        var summaryParts = new List<string>();

        var topTheme = themes.OrderByDescending(t => t.Strength).FirstOrDefault();
        if (topTheme != null)
        {
            summaryParts.Add(
                $"{FormatTheme(topTheme.Theme)} is the strongest theme ({topTheme.Direction.ToLowerInvariant()}, strength {topTheme.Strength}).");
        }

        if (marketAlignment.Count > 0)
        {
            var alignment = string.Join(", ", marketAlignment.Select(kvp => $"{kvp.Key.ToUpperInvariant()} {kvp.Value.ToLowerInvariant()}"));
            summaryParts.Add($"Market alignment: {alignment}.");
        }

        if (summaryParts.Count == 0)
        {
            summaryParts.Add("No active exogenous narratives were strong enough to influence decisions.");
        }

        return $"TL;DR: {string.Join(" ", summaryParts)}";
    }

    private static string NormalizeTag(string tag)
    {
        var normalized = tag.Replace("_", " ").Trim();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
    }

    private static string FormatTheme(string theme)
    {
        return theme.Replace("_", " ");
    }

    private static List<string> SplitNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return new List<string>();
        }

        return notes.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(note => note.TrimStart('-', ' ').Trim())
            .Where(note => !string.IsNullOrWhiteSpace(note))
            .ToList();
    }

    private static Dictionary<string, TValue> DeserializeOrDefault<TValue>(string json, Dictionary<string, TValue> fallback)
    {
        if (TryDeserialize(json, out Dictionary<string, TValue> result))
        {
            return result;
        }

        return fallback;
    }

    private static bool TryDeserialize<T>(string json, out T value)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>();
            return true;
        }
        catch (JsonException)
        {
            value = Activator.CreateInstance<T>();
            return false;
        }
    }
}
