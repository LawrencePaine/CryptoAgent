using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;

namespace CryptoAgent.Api.Services.Exogenous;

public record ExogenousItemContribution(
    Guid ItemId,
    Guid NarrativeId,
    decimal Contribution,
    decimal SourceWeight,
    decimal ConfidenceWeight,
    decimal HorizonWeight,
    decimal TimeDecay,
    ExogenousDirectionalBias DirectionalBias,
    ExogenousImpactHorizon ImpactHorizon);

public static class ExogenousScoring
{
    public static ExogenousItemContribution? ComputeContribution(
        ExogenousItemEntity item,
        ExogenousClassificationEntity classification,
        Guid narrativeId,
        DateTime asOfUtc)
    {
        if (!Enum.TryParse<ExogenousImpactHorizon>(classification.ImpactHorizon, true, out var horizon))
        {
            horizon = ExogenousImpactHorizon.NOISE;
        }

        if (!Enum.TryParse<ExogenousDirectionalBias>(classification.DirectionalBias, true, out var bias))
        {
            bias = ExogenousDirectionalBias.NEUTRAL;
        }

        var sourceWeight = Clamp01(item.SourceCredibilityWeight);
        var confidenceWeight = Clamp01(classification.ConfidenceScore);
        var horizonWeight = GetHorizonWeight(horizon);
        var timeDecay = GetTimeDecay(item.PublishedAt, asOfUtc, GetHalfLifeDays(horizon));
        var sign = GetDirectionalSign(bias);

        var contribution = sourceWeight * confidenceWeight * horizonWeight * timeDecay * sign;

        return new ExogenousItemContribution(
            item.Id,
            narrativeId,
            contribution,
            sourceWeight,
            confidenceWeight,
            horizonWeight,
            timeDecay,
            bias,
            horizon);
    }

    public static decimal GetHorizonWeight(ExogenousImpactHorizon horizon)
    {
        return horizon switch
        {
            ExogenousImpactHorizon.STRUCTURAL => 1.0m,
            ExogenousImpactHorizon.TRANSITIONAL => 0.6m,
            _ => 0.2m
        };
    }

    public static decimal GetHalfLifeDays(ExogenousImpactHorizon horizon)
    {
        return horizon switch
        {
            ExogenousImpactHorizon.STRUCTURAL => 21m,
            ExogenousImpactHorizon.TRANSITIONAL => 7m,
            _ => 2m
        };
    }

    public static decimal GetTimeDecay(DateTime publishedAtUtc, DateTime asOfUtc, decimal halfLifeDays)
    {
        var ageDays = Math.Max(0, (asOfUtc - publishedAtUtc).TotalDays);
        var lambda = Math.Log(2) / (double)halfLifeDays;
        var decay = Math.Exp(-lambda * ageDays);
        return (decimal)decay;
    }

    public static decimal GetDirectionalSign(ExogenousDirectionalBias bias)
    {
        return bias switch
        {
            ExogenousDirectionalBias.SUPPORTIVE => 1m,
            ExogenousDirectionalBias.ADVERSE => -1m,
            _ => 0m
        };
    }

    private static decimal Clamp01(decimal value)
    {
        if (value < 0m)
        {
            return 0m;
        }

        return value > 1m ? 1m : value;
    }
}
