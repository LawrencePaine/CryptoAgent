using System.Text.Json;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;
using Serilog;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousDecisionInputsPublisher
{
    private const decimal StrengthEpsilon = 0.01m;
    private const decimal ConflictOverrideThreshold = 0.7m;
    private const decimal StrengthAlignmentThreshold = 30m;
    private const double StrengthSquashK = 0.7;

    private readonly NarrativeRepository _narrativeRepository;
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;
    private readonly ExogenousItemRepository _itemRepository;
    private readonly ExogenousClassificationRepository _classificationRepository;
    private readonly HourlyFeatureRepository _featureRepository;
    private readonly RegimeStateRepository _regimeRepository;

    public ExogenousDecisionInputsPublisher(
        NarrativeRepository narrativeRepository,
        DecisionInputsExogenousRepository decisionInputsRepository,
        ExogenousItemRepository itemRepository,
        ExogenousClassificationRepository classificationRepository,
        HourlyFeatureRepository featureRepository,
        RegimeStateRepository regimeRepository)
    {
        _narrativeRepository = narrativeRepository;
        _decisionInputsRepository = decisionInputsRepository;
        _itemRepository = itemRepository;
        _classificationRepository = classificationRepository;
        _featureRepository = featureRepository;
        _regimeRepository = regimeRepository;
    }

    public async Task PublishAsync(DateTime timestampUtc, CancellationToken ct)
    {
        var windowStart = timestampUtc.AddDays(-30);
        var narratives = await _narrativeRepository.GetActiveNarrativesAsync(windowStart);
        var narrativeIds = narratives.Select(n => n.Id).ToList();
        var narrativeItems = await _narrativeRepository.GetNarrativeItemsAsync(narrativeIds);
        var itemIds = narrativeItems.Select(n => n.ItemId).Distinct().ToList();
        var items = await _itemRepository.GetItemsByIdsAsync(itemIds);
        var classifications = await _classificationRepository.GetByItemIdsAsync(itemIds);
        var itemById = items.ToDictionary(i => i.Id, i => i);
        var classificationByItem = classifications.ToDictionary(c => c.ItemId, c => c);

        var contributions = new List<ExogenousItemContribution>();
        var narrativeScores = new Dictionary<Guid, NarrativeScore>();

        foreach (var narrative in narratives)
        {
            decimal raw = 0m;
            decimal magnitude = 0m;
            foreach (var narrativeItem in narrativeItems.Where(n => n.NarrativeId == narrative.Id))
            {
                if (!itemById.TryGetValue(narrativeItem.ItemId, out var item))
                {
                    continue;
                }

                if (!classificationByItem.TryGetValue(narrativeItem.ItemId, out var classification))
                {
                    continue;
                }

                var contribution = ExogenousScoring.ComputeContribution(item, classification, narrative.Id, timestampUtc);
                if (contribution == null)
                {
                    continue;
                }

                raw += contribution.Contribution;
                magnitude += Math.Abs(contribution.Contribution);
                contributions.Add(contribution);
            }

            narrativeScores[narrative.Id] = new NarrativeScore(narrative, raw, magnitude);
        }

        var themeMetrics = BuildThemeMetrics(narratives, narrativeScores);
        var themeStrength = themeMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Strength);
        var themeDirection = themeMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Direction);
        var themeConflict = themeMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Conflict);

        var marketSnapshots = await BuildMarketSnapshotsAsync(timestampUtc);
        var gatingReasons = new Dictionary<string, List<string>>();
        var marketAlignment = new Dictionary<string, string>();

        foreach (var (asset, theme) in GetAssetThemeMap())
        {
            var reasons = new List<string>();
            var alignment = EvaluateAlignment(theme, themeMetrics, marketSnapshots, reasons);
            marketAlignment[asset] = alignment;
            gatingReasons[asset] = reasons;
        }

        var modifiers = ComputeModifiers(themeMetrics, marketAlignment, gatingReasons);

        var traceIds = BuildTraceIds(narrativeScores, contributions);
        var notes = BuildNotes(narrativeScores, themeMetrics, marketAlignment, modifiers);

        var entity = new DecisionInputsExogenousEntity
        {
            TimestampUtc = timestampUtc,
            ThemeScoresJson = JsonSerializer.Serialize(themeStrength),
            ThemeStrengthJson = JsonSerializer.Serialize(themeStrength),
            ThemeDirectionJson = JsonSerializer.Serialize(themeDirection),
            ThemeConflictJson = JsonSerializer.Serialize(themeConflict),
            AlignmentFlagsJson = JsonSerializer.Serialize(themeDirection),
            MarketAlignmentJson = JsonSerializer.Serialize(marketAlignment),
            GatingReasonJson = JsonSerializer.Serialize(gatingReasons),
            AbstainModifier = modifiers.abstain,
            ConfidenceThresholdModifier = modifiers.threshold,
            PositionSizeModifier = modifiers.positionSize,
            Notes = notes,
            TraceIdsJson = JsonSerializer.Serialize(traceIds)
        };

        await _decisionInputsRepository.UpsertAsync(entity);

        Log.Information(
            "Decision inputs exogenous published {TimestampUtc} AI={AIScore} ETH={EthScore} abstain={Abstain} threshold={Threshold}",
            timestampUtc,
            themeStrength.GetValueOrDefault("AI_COMPUTE"),
            themeStrength.GetValueOrDefault("ETH_ECOSYSTEM"),
            modifiers.abstain,
            modifiers.threshold);
    }

    private static Dictionary<string, ThemeMetric> BuildThemeMetrics(
        IEnumerable<NarrativeEntity> narratives,
        Dictionary<Guid, NarrativeScore> narrativeScores)
    {
        var metrics = new Dictionary<string, ThemeMetric>();
        foreach (var theme in new[] { "AI_COMPUTE", "ETH_ECOSYSTEM" })
        {
            var relevant = narratives.Where(n => n.Theme == theme).ToList();
            var raw = relevant.Sum(n => narrativeScores.GetValueOrDefault(n.Id)?.Raw ?? 0m);
            var magnitude = relevant.Sum(n => narrativeScores.GetValueOrDefault(n.Id)?.Magnitude ?? 0m);

            var direction = raw switch
            {
                > StrengthEpsilon => "SUPPORTIVE",
                < -StrengthEpsilon => "ADVERSE",
                _ => "NEUTRAL"
            };

            var strength = magnitude <= 0m
                ? 0m
                : (decimal)(100 * (1 - Math.Exp(-StrengthSquashK * (double)magnitude)));

            var conflict = magnitude <= 0m
                ? 0m
                : 1m - (Math.Abs(raw) / (magnitude + 0.0001m));

            metrics[theme] = new ThemeMetric(raw, magnitude, strength, conflict, direction);
        }

        return metrics;
    }

    private async Task<Dictionary<string, MarketContextSnapshot?>> BuildMarketSnapshotsAsync(DateTime timestampUtc)
    {
        var snapshots = new Dictionary<string, MarketContextSnapshot?>();
        foreach (var asset in new[] { "BTC", "ETH" })
        {
            var feature = await _featureRepository.GetLatestAsync(asset);
            var regime = await _regimeRepository.GetLatestAsync(asset);
            snapshots[asset] = feature == null || regime == null
                ? null
                : new MarketContextSnapshot
                {
                    Asset = asset,
                    TimestampUtc = timestampUtc,
                    Regime = regime.Regime,
                    VolatilityScore = feature.Vol24h,
                    TrendScore = feature.TrendStrength,
                    MomentumScore = feature.MomentumScore
                };
        }

        return snapshots;
    }

    private static Dictionary<string, string> GetAssetThemeMap()
    {
        return new Dictionary<string, string>
        {
            { "BTC", "AI_COMPUTE" },
            { "ETH", "ETH_ECOSYSTEM" }
        };
    }

    private static string EvaluateAlignment(
        string theme,
        Dictionary<string, ThemeMetric> themeMetrics,
        Dictionary<string, MarketContextSnapshot?> snapshots,
        List<string> reasons)
    {
        if (!themeMetrics.TryGetValue(theme, out var metric))
        {
            reasons.Add("Theme metrics unavailable.");
            return "UNKNOWN";
        }

        if (metric.Strength < StrengthAlignmentThreshold)
        {
            reasons.Add($"Strength {metric.Strength:F0} < {StrengthAlignmentThreshold:F0}.");
            return "UNKNOWN";
        }

        if (metric.Conflict > ConflictOverrideThreshold)
        {
            reasons.Add($"Conflict {metric.Conflict:F2} > {ConflictOverrideThreshold:F2}.");
            return "UNKNOWN";
        }

        var asset = GetAssetThemeMap().First(kvp => kvp.Value == theme).Key;
        if (!snapshots.TryGetValue(asset, out var snapshot) || snapshot == null)
        {
            reasons.Add("Market snapshot unavailable.");
            return "UNKNOWN";
        }

        var regime = snapshot.Regime;
        var isTrendUp = regime.Contains("TrendUp", StringComparison.OrdinalIgnoreCase)
            || snapshot.TrendScore >= 0.01m
            || snapshot.MomentumScore >= 0.2m;
        var isTrendDown = snapshot.TrendScore <= -0.01m
            || snapshot.MomentumScore <= -0.2m
            || (snapshot.VolatilityScore >= 0.02m && snapshot.MomentumScore < 0m);
        var isRange = regime.Contains("Range", StringComparison.OrdinalIgnoreCase);

        reasons.Add($"Regime {regime}, trend {snapshot.TrendScore:F2}, momentum {snapshot.MomentumScore:F2}, vol {snapshot.VolatilityScore:P2}.");

        return metric.Direction switch
        {
            "SUPPORTIVE" when isTrendUp => "ALIGNED",
            "SUPPORTIVE" when isTrendDown => "MISALIGNED",
            "ADVERSE" when isTrendDown => "ALIGNED",
            "ADVERSE" when isTrendUp => "MISALIGNED",
            _ when isRange => "UNKNOWN",
            _ => "UNKNOWN"
        };
    }

    private static (decimal abstain, decimal threshold, decimal positionSize) ComputeModifiers(
        Dictionary<string, ThemeMetric> themeMetrics,
        Dictionary<string, string> marketAlignment,
        Dictionary<string, List<string>> gatingReasons)
    {
        var abstain = 0m;
        var threshold = 0m;
        var positionSize = 1m;

        foreach (var (asset, theme) in GetAssetThemeMap())
        {
            if (!themeMetrics.TryGetValue(theme, out var metric))
            {
                continue;
            }

            if (!marketAlignment.TryGetValue(asset, out var alignment))
            {
                alignment = "UNKNOWN";
            }

            switch (alignment)
            {
                case "ALIGNED":
                    if (metric.Strength >= 60m && metric.Conflict <= 0.3m)
                    {
                        abstain -= 0.03m;
                        threshold -= 0.02m;
                        gatingReasons[asset].Add("Aligned with strong, low-conflict theme.");
                    }
                    else
                    {
                        abstain -= 0.01m;
                        threshold -= 0.01m;
                        gatingReasons[asset].Add("Aligned with moderate theme strength.");
                    }
                    break;
                case "MISALIGNED":
                    abstain += 0.15m;
                    threshold += 0.10m;
                    positionSize = Math.Min(positionSize, 0.7m);
                    gatingReasons[asset].Add("Theme direction conflicts with market regime.");
                    break;
                default:
                    abstain += 0.05m;
                    gatingReasons[asset].Add("Alignment unknown; nudging abstain higher.");
                    break;
            }
        }

        abstain = Math.Clamp(abstain, 0m, 0.25m);
        threshold = Math.Clamp(threshold, -0.05m, 0.15m);
        positionSize = Math.Clamp(positionSize, 0.5m, 1m);

        return (abstain, threshold, positionSize);
    }

    private static List<string> BuildTraceIds(
        Dictionary<Guid, NarrativeScore> narrativeScores,
        List<ExogenousItemContribution> contributions)
    {
        var topNarratives = narrativeScores.Values
            .OrderByDescending(n => n.Magnitude)
            .Take(3)
            .Select(n => n.Narrative.Id)
            .ToList();

        var topItems = contributions
            .OrderByDescending(c => Math.Abs(c.Contribution))
            .Take(5)
            .Select(c => c.ItemId)
            .ToList();

        var traceIds = new List<string>();
        traceIds.AddRange(topNarratives.Select(id => $"narrative:{id}"));
        traceIds.AddRange(topItems.Select(id => $"item:{id}"));

        return traceIds;
    }

    private static string BuildNotes(
        Dictionary<Guid, NarrativeScore> narrativeScores,
        Dictionary<string, ThemeMetric> themeMetrics,
        Dictionary<string, string> marketAlignment,
        (decimal abstain, decimal threshold, decimal positionSize) modifiers)
    {
        var bullets = new List<string>();
        var topNarratives = narrativeScores.Values
            .OrderByDescending(n => n.Magnitude)
            .Take(3)
            .ToList();

        foreach (var narrative in topNarratives)
        {
            bullets.Add($"{narrative.Narrative.Theme}: {narrative.Narrative.Label} ({narrative.Raw:+0.00;-0.00;+0.00})");
        }

        foreach (var metric in themeMetrics)
        {
            bullets.Add($"{metric.Key}: strength {metric.Value.Strength:F0}, direction {metric.Value.Direction}, conflict {metric.Value.Conflict:F2}.");
        }

        foreach (var alignment in marketAlignment)
        {
            bullets.Add($"Market alignment {alignment.Key}: {alignment.Value}.");
        }

        bullets.Add($"Modifiers - abstain {modifiers.abstain:+0.00;-0.00;+0.00}, threshold {modifiers.threshold:+0.00;-0.00;+0.00}, position size {modifiers.positionSize:F2}.");

        return string.Join("\n", bullets);
    }

    private record NarrativeScore(NarrativeEntity Narrative, decimal Raw, decimal Magnitude);

    private record ThemeMetric(decimal Raw, decimal Magnitude, decimal Strength, decimal Conflict, string Direction);
}
