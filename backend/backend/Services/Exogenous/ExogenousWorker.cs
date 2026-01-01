using CryptoAgent.Api.Models;
using CryptoAgent.Api.Worker.Jobs.Exogenous;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerConfig _workerConfig;

    public ExogenousWorker(IServiceScopeFactory scopeFactory, WorkerConfig workerConfig)
    {
        _scopeFactory = scopeFactory;
        _workerConfig = workerConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateTime? lastIngestHour = null;
        DateTime? lastClassifyHour = null;
        DateTime? lastAggregateHour = null;
        DateTime? lastPublishHour = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

            using var scope = _scopeFactory.CreateScope();

            if (now.Minute == _workerConfig.ExogenousIngestMinute && lastIngestHour != currentHour)
            {
                var job = scope.ServiceProvider.GetRequiredService<ExogenousIngestionJob>();
                await job.RunAsync(stoppingToken);
                lastIngestHour = currentHour;
            }

            if (now.Minute == _workerConfig.ExogenousClassifyMinute && lastClassifyHour != currentHour)
            {
                var job = scope.ServiceProvider.GetRequiredService<ExogenousClassificationJob>();
                await job.RunAsync(_workerConfig.ExogenousClassificationBatchSize, stoppingToken);
                lastClassifyHour = currentHour;
            }

            if (now.Minute == _workerConfig.ExogenousNarrativeMinute && lastAggregateHour != currentHour)
            {
                var job = scope.ServiceProvider.GetRequiredService<ExogenousNarrativeAggregationJob>();
                await job.RunAsync(stoppingToken);
                lastAggregateHour = currentHour;
            }

            if (now.Minute == _workerConfig.ExogenousDecisionInputsMinute && lastPublishHour != currentHour)
            {
                var job = scope.ServiceProvider.GetRequiredService<ExogenousDecisionInputsJob>();
                await job.RunAsync(currentHour, stoppingToken);
                lastPublishHour = currentHour;
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
