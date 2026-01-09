using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, ExogenousRefreshJobStatus> _jobs = new();
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private DateTime? _lastRefreshUtc;
    private bool _running;
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(2);

    public ExogenousRefreshService(
        IServiceScopeFactory scopeFactory,
        WorkerConfig workerConfig,
        ILogger<ExogenousRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _workerConfig = workerConfig;
        _logger = logger;
    }

    public async Task<ExogenousRefreshStartResult> StartRefreshAsync(CancellationToken ct)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            if (_running)
            {
                return ExogenousRefreshStartResult.AlreadyRunning();
            }

            if (_lastRefreshUtc.HasValue && DateTime.UtcNow - _lastRefreshUtc.Value < Cooldown)
            {
                return ExogenousRefreshStartResult.Cooldown(_lastRefreshUtc.Value + Cooldown);
            }

            var jobId = Guid.NewGuid().ToString("N");
            var status = new ExogenousRefreshJobStatus
            {
                JobId = jobId,
                Status = ExogenousRefreshStatus.Queued,
                QueuedUtc = DateTime.UtcNow
            };
            _jobs[jobId] = status;
            _running = true;

            _ = Task.Run(() => RunRefreshAsync(jobId, CancellationToken.None));

            return ExogenousRefreshStartResult.Enqueued(jobId);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public ExogenousRefreshJobStatus? GetStatus(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var status) ? status : null;
    }

    private async Task RunRefreshAsync(string jobId, CancellationToken ct)
    {
        var startedUtc = DateTime.UtcNow;
        try
        {
            UpdateStatus(jobId, status =>
            {
                status.Status = ExogenousRefreshStatus.Running;
                status.StartedUtc = startedUtc;
            });

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

            var narratives = await aggregationJob.RunAsync(ct);
            var tickUtc = NormalizeTickUtc(DateTime.UtcNow);
            await decisionInputsJob.RunAsync(tickUtc, ct);

            var finishedUtc = DateTime.UtcNow;
            var duration = (long)(finishedUtc - startedUtc).TotalMilliseconds;

            UpdateStatus(jobId, status =>
            {
                status.Status = ExogenousRefreshStatus.Succeeded;
                status.FinishedUtc = finishedUtc;
                status.ResultSummary = new ExogenousRefreshResultSummary
                {
                    TickUtc = tickUtc,
                    ItemsIngested = ingestCount,
                    ItemsClassified = classified,
                    NarrativesAggregated = narratives,
                    DurationMs = duration
                };
            });

            _lastRefreshUtc = finishedUtc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exogenous refresh job {JobId} failed", jobId);
            UpdateStatus(jobId, status =>
            {
                status.Status = ExogenousRefreshStatus.Failed;
                status.FinishedUtc = DateTime.UtcNow;
                status.Error = ex.Message;
            });
        }
        finally
        {
            _running = false;
        }
    }

    private void UpdateStatus(string jobId, Action<ExogenousRefreshJobStatus> update)
    {
        _jobs.AddOrUpdate(jobId,
            _ => new ExogenousRefreshJobStatus { JobId = jobId },
            (_, existing) =>
            {
                update(existing);
                return existing;
            });
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

public record ExogenousRefreshStartResult
{
    public string? JobId { get; init; }
    public ExogenousRefreshStartStatus Status { get; init; }
    public DateTime? NextAvailableUtc { get; init; }

    public static ExogenousRefreshStartResult Enqueued(string jobId) => new()
    {
        JobId = jobId,
        Status = ExogenousRefreshStartStatus.Enqueued
    };

    public static ExogenousRefreshStartResult AlreadyRunning() => new()
    {
        Status = ExogenousRefreshStartStatus.AlreadyRunning
    };

    public static ExogenousRefreshStartResult Cooldown(DateTime nextAvailableUtc) => new()
    {
        Status = ExogenousRefreshStartStatus.Cooldown,
        NextAvailableUtc = nextAvailableUtc
    };
}
