using CryptoAgent.Api.Backtesting.Engine;

namespace CryptoAgent.Api.Backtesting.WalkForward;

public class WalkForwardRunner
{
    public record WalkForwardConfig(int WarmupHours, int TrainDays, int TestDays, int StepDays);

    public async Task<List<WalkForwardSegmentResult>> RunAsync(WalkForwardConfig config, Func<BacktestConfig, Task<BacktestResult>> backtestFactory)
    {
        // Placeholder implementation: returns empty segments to keep pipeline compilable.
        return await Task.FromResult(new List<WalkForwardSegmentResult>());
    }
}

public record WalkForwardSegmentResult(DateTime TrainStart, DateTime TrainEnd, DateTime TestStart, DateTime TestEnd, BacktestResult? TestResult);
