using System.Text.Json;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Repositories;
using Serilog;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousDecisionInputsPublisher
{
    private readonly NarrativeRepository _narrativeRepository;
    private readonly DecisionInputsExogenousRepository _decisionInputsRepository;

    public ExogenousDecisionInputsPublisher(
        NarrativeRepository narrativeRepository,
        DecisionInputsExogenousRepository decisionInputsRepository)
    {
        _narrativeRepository = narrativeRepository;
        _decisionInputsRepository = decisionInputsRepository;
    }

    public async Task PublishAsync(DateTime timestampUtc, CancellationToken ct)
    {
        var windowStart = timestampUtc.AddDays(-30);
        var narratives = await _narrativeRepository.GetActiveNarrativesAsync(windowStart);

        var themeScores = new Dictionary<string, decimal>
        {
            { "AI_COMPUTE", ScoreTheme(narratives, "AI_COMPUTE") },
            { "ETH_ECOSYSTEM", ScoreTheme(narratives, "ETH_ECOSYSTEM") }
        };

        var alignmentFlags = new Dictionary<string, string>
        {
            { "AI_COMPUTE", AlignmentFlag(narratives, "AI_COMPUTE") },
            { "ETH_ECOSYSTEM", AlignmentFlag(narratives, "ETH_ECOSYSTEM") }
        };

        var modifiers = ComputeModifiers(narratives, themeScores);
        var traceIds = await BuildTraceIdsAsync(narratives);
        var notes = BuildNotes(narratives, themeScores, modifiers);

        var entity = new DecisionInputsExogenousEntity
        {
            TimestampUtc = timestampUtc,
            ThemeScoresJson = JsonSerializer.Serialize(themeScores),
            AlignmentFlagsJson = JsonSerializer.Serialize(alignmentFlags),
            AbstainModifier = modifiers.abstain,
            ConfidenceThresholdModifier = modifiers.threshold,
            Notes = notes,
            TraceIdsJson = JsonSerializer.Serialize(traceIds)
        };

        await _decisionInputsRepository.UpsertAsync(entity);

        Log.Information(
            "Decision inputs exogenous published {TimestampUtc} AI={AIScore} ETH={EthScore} abstain={Abstain} threshold={Threshold}",
            timestampUtc,
            themeScores["AI_COMPUTE"],
            themeScores["ETH_ECOSYSTEM"],
            modifiers.abstain,
            modifiers.threshold);
    }

    private static decimal ScoreTheme(IEnumerable<NarrativeEntity> narratives, string theme)
    {
        var themeNarratives = narratives.Where(n => n.Theme == theme).OrderByDescending(n => n.StateScore).Take(3).ToList();
        if (themeNarratives.Count == 0)
        {
            return 0;
        }

        return Math.Min(100, themeNarratives.Average(n => n.StateScore));
    }

    private static string AlignmentFlag(IEnumerable<NarrativeEntity> narratives, string theme)
    {
        var top = narratives.Where(n => n.Theme == theme).OrderByDescending(n => n.StateScore).FirstOrDefault();
        if (top == null || top.StateScore < 40)
        {
            return "NEUTRAL";
        }

        return top.DirectionalBias switch
        {
            "SUPPORTIVE" => "BULLISH_BIAS",
            "ADVERSE" => "BEARISH_BIAS",
            _ => "MIXED"
        };
    }

    private static (decimal abstain, decimal threshold) ComputeModifiers(
        IEnumerable<NarrativeEntity> narratives,
        Dictionary<string, decimal> themeScores)
    {
        var abstain = 0m;
        var threshold = 0m;

        var aiScore = themeScores["AI_COMPUTE"];
        var ethScore = themeScores["ETH_ECOSYSTEM"];

        if (aiScore < 40 && ethScore < 40)
        {
            abstain += 0.12m;
            threshold += 0.05m;
        }

        var adverseStructural = narratives.Any(n => n.DirectionalBias == "ADVERSE" && n.Horizon == "STRUCTURAL" && n.StateScore > 60);
        if (adverseStructural)
        {
            abstain += 0.12m;
            threshold += 0.05m;
        }

        var supportiveStructural = narratives.Any(n => n.DirectionalBias == "SUPPORTIVE" && n.Horizon == "STRUCTURAL" && n.StateScore > 70);
        if (supportiveStructural && !adverseStructural)
        {
            threshold -= 0.03m;
        }

        var opposing = narratives.Any(n => n.DirectionalBias == "SUPPORTIVE") && narratives.Any(n => n.DirectionalBias == "ADVERSE");
        if (opposing)
        {
            abstain += 0.08m;
        }

        return (abstain, threshold);
    }

    private async Task<List<string>> BuildTraceIdsAsync(IEnumerable<NarrativeEntity> narratives)
    {
        var topNarratives = narratives.OrderByDescending(n => n.StateScore).Take(3).ToList();
        var narrativeIds = topNarratives.Select(n => n.Id).ToList();
        var narrativeItems = await _narrativeRepository.GetNarrativeItemsAsync(narrativeIds);
        var topItemIds = narrativeItems
            .OrderByDescending(i => i.ContributionWeight)
            .Take(5)
            .Select(i => i.ItemId)
            .ToList();

        var traceIds = new List<string>();
        traceIds.AddRange(narrativeIds.Select(id => $"narrative:{id}"));
        traceIds.AddRange(topItemIds.Select(id => $"item:{id}"));

        return traceIds;
    }

    private static string BuildNotes(
        IEnumerable<NarrativeEntity> narratives,
        Dictionary<string, decimal> themeScores,
        (decimal abstain, decimal threshold) modifiers)
    {
        var bullets = new List<string>();
        var topNarratives = narratives.OrderByDescending(n => n.StateScore).Take(3).ToList();

        foreach (var narrative in topNarratives)
        {
            bullets.Add($"{narrative.Theme}: {narrative.Label} ({narrative.DirectionalBias}, {narrative.Horizon}, {narrative.StateScore:F0})");
        }

        bullets.Add($"Theme scores - AI_COMPUTE: {themeScores["AI_COMPUTE"]:F0}, ETH_ECOSYSTEM: {themeScores["ETH_ECOSYSTEM"]:F0}.");
        bullets.Add($"Modifiers - abstain {modifiers.abstain:+0.00;-0.00;+0.00}, threshold {modifiers.threshold:+0.00;-0.00;+0.00}.");

        return string.Join("\n", bullets);
    }
}
