using CryptoAgent.Api.Models;
using CryptoAgent.Api.Worker.Jobs.Exogenous;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousRefreshService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerConfig _workerConfig;
    private readonly ILogger<ExogenousRefreshService> _logger;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private DateTime? _lastRefreshUtc;
    private bool _running;
    private DateTime? _currentStartedUtc;

    public ExogenousRefreshService(
        IServiceScopeFactory scopeFactory,
        WorkerConfig workerConfig,
        ILogger<ExogenousRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _workerConfig = workerConfig;
        _logger = logger;
    }

    public async Task<ExogenousRefreshResponse> RefreshAsync(CancellationToken ct)
    {
        await _mutex.WaitAsync(ct);
        if (_running)
        {
            _mutex.Release();
            return new ExogenousRefreshResponse
            {
                Status = ExogenousRefreshStatus.Running,
                StartedUtc = _currentStartedUtc,
                LastRefreshUtc = _lastRefreshUtc,
                Message = "Refresh already running."
            };
        }

        _running = true;
        _currentStartedUtc = DateTime.UtcNow;
        _mutex.Release();

        var startedUtc = _currentStartedUtc.Value;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var ingestionJob = scope.ServiceProvider.GetRequiredService<ExogenousIngestionJob>();
            var classificationJob = scope.ServiceProvider.GetRequiredService<ExogenousClassificationJob>();
            var aggregationJob = scope.ServiceProvider.GetRequiredService<ExogenousNarrativeAggregationJob>();
            var decisionInputsJob = scope.ServiceProvider.GetRequiredService<ExogenousDecisionInputsJob>();

            var ingestCount = await ingestionJob.RunAsync(ct);
            var classified = 0;
            int batchCount;
            do
            {
                batchCount = await classificationJob.RunAsync(_workerConfig.ExogenousClassificationBatchSize, ct);
                classified += batchCount;
            } while (batchCount == _workerConfig.ExogenousClassificationBatchSize);

            await aggregationJob.RunAsync(ct);
            var tickUtc = NormalizeTickUtc(DateTime.UtcNow);
            await decisionInputsJob.RunAsync(tickUtc, ct);

            var finishedUtc = DateTime.UtcNow;
            _lastRefreshUtc = finishedUtc;

            return new ExogenousRefreshResponse
            {
                Status = ExogenousRefreshStatus.Succeeded,
                StartedUtc = startedUtc,
                FinishedUtc = finishedUtc,
                LastRefreshUtc = finishedUtc,
                ItemsIngested = ingestCount,
                ItemsClassified = classified
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exogenous refresh failed");
            return new ExogenousRefreshResponse
            {
                Status = ExogenousRefreshStatus.Failed,
                StartedUtc = startedUtc,
                FinishedUtc = DateTime.UtcNow,
                LastRefreshUtc = _lastRefreshUtc,
                ItemsIngested = 0,
                ItemsClassified = 0,
                Message = ex.Message
            };
        }
        finally
        {
            _running = false;
            _currentStartedUtc = null;
        }
    }

    private static DateTime NormalizeTickUtc(DateTime tickUtc)
    {
        var utc = tickUtc.Kind == DateTimeKind.Utc
            ? tickUtc
            : tickUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(tickUtc, DateTimeKind.Utc)
                : tickUtc.ToUniversalTime();

        return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
    }
}
