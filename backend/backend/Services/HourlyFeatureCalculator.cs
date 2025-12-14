using CryptoAgent.Api.Data.Entities;

namespace CryptoAgent.Api.Services;

public class HourlyFeatureCalculator
{
    public HourlyFeatureEntity? CalculateLatest(string asset, IList<HourlyCandleEntity> candles)
    {
        if (candles.Count < 2)
        {
            return null;
        }

        var ordered = candles.OrderBy(c => c.HourUtc).ToList();
        var latest = ordered.Last();
        var latestIndex = ordered.Count - 1;

        decimal close = latest.CloseGbp;
        decimal prevClose = ordered[latestIndex - 1].CloseGbp;
        var feature = new HourlyFeatureEntity
        {
            Asset = asset,
            HourUtc = latest.HourUtc,
            Return1h = SafeReturn(close, prevClose),
            IsComplete = true
        };

        feature.Return24h = CalculateReturn(ordered, 24, latestIndex, ref feature.IsComplete, close);
        feature.Return7d = CalculateReturn(ordered, 168, latestIndex, ref feature.IsComplete, close);

        feature.Sma24 = CalculateSma(ordered, 24, latestIndex, ref feature.IsComplete);
        feature.Sma168 = CalculateSma(ordered, 168, latestIndex, ref feature.IsComplete);

        feature.Vol24h = CalculateVol(ordered, 24, latestIndex, ref feature.IsComplete);
        feature.Vol72h = CalculateVol(ordered, 72, latestIndex, ref feature.IsComplete);

        feature.TrendStrength = feature.Sma168 == 0 ? 0 : (feature.Sma24 / feature.Sma168) - 1;
        feature.Drawdown7d = CalculateDrawdown(ordered, 168, latestIndex, close, ref feature.IsComplete);

        var momentum = 0.5m * feature.Return24h + 0.5m * feature.TrendStrength;
        feature.MomentumScore = Clamp(momentum * 10, -1, 1);

        return feature;
    }

    private static decimal CalculateReturn(List<HourlyCandleEntity> ordered, int lookback, int latestIndex, ref bool complete, decimal latestClose)
    {
        if (latestIndex - lookback < 0)
        {
            complete = false;
            return 0;
        }

        var reference = ordered[latestIndex - lookback].CloseGbp;
        return SafeReturn(latestClose, reference);
    }

    private static decimal CalculateSma(List<HourlyCandleEntity> ordered, int lookback, int latestIndex, ref bool complete)
    {
        if (latestIndex - lookback + 1 < 0)
        {
            complete = false;
            return 0;
        }

        var slice = ordered.Skip(latestIndex - lookback + 1).Take(lookback).Select(c => c.CloseGbp).ToList();
        return slice.Average();
    }

    private static decimal CalculateVol(List<HourlyCandleEntity> ordered, int lookback, int latestIndex, ref bool complete)
    {
        if (latestIndex - lookback + 1 < 1)
        {
            complete = false;
            return 0;
        }

        var returns = new List<decimal>();
        for (int i = latestIndex - lookback + 1; i <= latestIndex; i++)
        {
            var prev = ordered[i - 1].CloseGbp;
            var curr = ordered[i].CloseGbp;
            returns.Add(SafeReturn(curr, prev));
        }

        var mean = returns.Average();
        var variance = returns.Select(r => (r - mean) * (r - mean)).Average();
        return (decimal)Math.Sqrt((double)variance);
    }

    private static decimal CalculateDrawdown(List<HourlyCandleEntity> ordered, int lookback, int latestIndex, decimal latestClose, ref bool complete)
    {
        if (latestIndex - lookback + 1 < 0)
        {
            complete = false;
            return 0;
        }

        var window = ordered.Skip(latestIndex - lookback + 1).Take(lookback).Select(c => c.CloseGbp).ToList();
        var maxClose = window.Max();
        return maxClose == 0 ? 0 : (latestClose / maxClose) - 1;
    }

    private static decimal SafeReturn(decimal current, decimal reference)
    {
        if (reference == 0) return 0;
        return (current / reference) - 1;
    }

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
