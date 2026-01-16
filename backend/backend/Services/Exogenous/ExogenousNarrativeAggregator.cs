using System.Text.Json;
using System.Text.RegularExpressions;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;
using Serilog;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousNarrativeAggregator
{
    private static readonly Regex TokenRegex = new("[A-Za-z0-9]+", RegexOptions.Compiled);
    private readonly ExogenousItemRepository _itemRepository;
    private readonly ExogenousClassificationRepository _classificationRepository;
    private readonly NarrativeRepository _narrativeRepository;

    public ExogenousNarrativeAggregator(
        ExogenousItemRepository itemRepository,
        ExogenousClassificationRepository classificationRepository,
        NarrativeRepository narrativeRepository)
    {
        _itemRepository = itemRepository;
        _classificationRepository = classificationRepository;
        _narrativeRepository = narrativeRepository;
    }

    public async Task<int> AggregateAsync(CancellationToken ct)
    {
        var items = await _itemRepository.GetClassifiedUnassignedAsync(200);
        if (items.Count == 0)
        {
            return 0;
        }

        var classifications = await _classificationRepository.GetByItemIdsAsync(items.Select(i => i.Id));
        var classificationByItem = classifications
            .GroupBy(c => c.ItemId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CreatedAt).First());
        var added = 0;
        var windowStart = DateTime.UtcNow.AddDays(-30);

        foreach (var item in items)
        {
            if (!classificationByItem.TryGetValue(item.Id, out var classification))
            {
                continue;
            }

            var theme = classification.ThemeRelevance;
            if (theme == "NONE")
            {
                continue;
            }

            var narratives = await _narrativeRepository.GetActiveByThemeAsync(theme, windowStart);
            var itemText = BuildItemText(item, classification);
            NarrativeEntity? matched = null;
            var maxSimilarity = 0d;

            foreach (var narrative in narratives)
            {
                var similarity = CosineSimilarity(narrative.SeedText, itemText);
                if (similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    matched = narrative;
                }
            }

            if (matched == null || maxSimilarity < 0.82)
            {
                matched = new NarrativeEntity
                {
                    Id = Guid.NewGuid(),
                    Theme = theme,
                    Label = Truncate(item.Title, 72),
                    SeedText = itemText,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow,
                    StateScore = 0,
                    DirectionalBias = classification.DirectionalBias,
                    Horizon = classification.ImpactHorizon,
                    IsActive = true
                };

                await _narrativeRepository.AddAsync(matched);
            }

            await _narrativeRepository.AddNarrativeItemAsync(new NarrativeItemEntity
            {
                NarrativeId = matched.Id,
                ItemId = item.Id,
                ContributionWeight = classification.ConfidenceScore * item.SourceCredibilityWeight,
                AddedAt = DateTime.UtcNow
            });

            await UpdateNarrativeStateAsync(matched, windowStart);
            added++;

            Log.Information("Narrative aggregation assigned item={ItemId} narrative={NarrativeId}", item.Id, matched.Id);
        }

        return added;
    }

    public async Task RebuildAsync(CancellationToken ct)
    {
        var windowStart = DateTime.UtcNow.AddDays(-30);
        await _narrativeRepository.ClearNarrativeItemsAsync(windowStart);

        foreach (var theme in new[] { "AI_COMPUTE", "ETH_ECOSYSTEM" })
        {
            await _narrativeRepository.ClearNarrativesAsync(theme, windowStart);
        }

        await AggregateAsync(ct);
    }

    private async Task UpdateNarrativeStateAsync(NarrativeEntity narrative, DateTime windowStart)
    {
        var narrativeItems = await _narrativeRepository.GetNarrativeItemsAsync(narrative.Id);
        if (narrativeItems.Count == 0)
        {
            return;
        }

        var itemIds = narrativeItems.Select(n => n.ItemId).ToList();
        var items = await _itemRepository.GetItemsByIdsAsync(itemIds);
        var classifications = await _classificationRepository.GetByItemIdsAsync(itemIds);
        var classificationByItem = classifications
            .GroupBy(c => c.ItemId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CreatedAt).First());

        var weightByBias = new Dictionary<string, decimal>();
        var weightByHorizon = new Dictionary<string, decimal>();
        decimal totalWeight = 0;

        foreach (var item in items)
        {
            if (item.PublishedAt < windowStart)
            {
                continue;
            }

            if (!classificationByItem.TryGetValue(item.Id, out var classification))
            {
                continue;
            }

            var horizon = classification.ImpactHorizon;
            var halfLifeDays = GetHalfLifeDays(horizon);
            var ageDays = Math.Max(0, (DateTime.UtcNow - item.PublishedAt).TotalDays);
            var decay = Math.Pow(0.5, ageDays / halfLifeDays);
            var weight = classification.ConfidenceScore * item.SourceCredibilityWeight * (decimal)decay;

            if (!weightByBias.ContainsKey(classification.DirectionalBias))
            {
                weightByBias[classification.DirectionalBias] = 0;
            }
            weightByBias[classification.DirectionalBias] += weight;

            if (!weightByHorizon.ContainsKey(horizon))
            {
                weightByHorizon[horizon] = 0;
            }
            weightByHorizon[horizon] += weight;

            totalWeight += weight;
        }

        narrative.StateScore = Math.Min(100, totalWeight * 100);
        narrative.DirectionalBias = weightByBias.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? narrative.DirectionalBias;
        narrative.Horizon = weightByHorizon.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? narrative.Horizon;
        narrative.LastUpdatedAt = DateTime.UtcNow;
        narrative.IsActive = narrative.StateScore >= 10;

        await _narrativeRepository.UpdateAsync(narrative);
    }

    private static double CosineSimilarity(string left, string right)
    {
        var vecA = Tokenize(left);
        var vecB = Tokenize(right);
        if (vecA.Count == 0 || vecB.Count == 0)
        {
            return 0;
        }

        var dot = 0d;
        var magA = 0d;
        var magB = 0d;

        foreach (var (token, count) in vecA)
        {
            var a = count;
            magA += a * a;
            if (vecB.TryGetValue(token, out var b))
            {
                dot += a * b;
            }
        }

        foreach (var count in vecB.Values)
        {
            magB += count * count;
        }

        if (magA == 0 || magB == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }

    private static Dictionary<string, double> Tokenize(string text)
    {
        var tokens = TokenRegex.Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(token => token.Length > 2)
            .ToList();

        var dict = new Dictionary<string, double>();
        foreach (var token in tokens)
        {
            dict[token] = dict.TryGetValue(token, out var count) ? count + 1 : 1;
        }

        return dict;
    }

    private static string BuildItemText(ExogenousItemEntity item, ExogenousClassificationEntity classification)
    {
        var bullets = JsonSerializer.Deserialize<List<string>>(classification.SummaryBulletsJson) ?? new List<string>();
        return string.Join(" ", item.Title, item.RawExcerpt, string.Join(" ", bullets));
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text.Length <= max ? text : text[..max].TrimEnd();
    }

    private static double GetHalfLifeDays(string horizon)
    {
        return horizon switch
        {
            "STRUCTURAL" => 21,
            "TRANSITIONAL" => 7,
            _ => 2
        };
    }
}
