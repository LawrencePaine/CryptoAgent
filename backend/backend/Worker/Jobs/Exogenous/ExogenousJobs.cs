using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services.Exogenous;

namespace CryptoAgent.Api.Worker.Jobs.Exogenous;

public class ExogenousIngestionJob
{
    private readonly ExogenousIngestionService _ingestionService;

    public ExogenousIngestionJob(ExogenousIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    public Task<int> RunAsync(CancellationToken ct) => _ingestionService.IngestAsync(ct);
}

public class ExogenousClassificationJob
{
    private readonly ExogenousItemRepository _itemRepository;
    private readonly IExogenousClassifier _classifier;

    public ExogenousClassificationJob(ExogenousItemRepository itemRepository, IExogenousClassifier classifier)
    {
        _itemRepository = itemRepository;
        _classifier = classifier;
    }

    public async Task<int> RunAsync(int limit, CancellationToken ct)
    {
        var items = await _itemRepository.GetByStatusAsync("NEW", limit);
        var processed = 0;

        foreach (var item in items)
        {
            try
            {
                await _classifier.ClassifyAsync(item, ct);
            }
            catch
            {
                // classification service handles status update
            }
            processed++;
        }

        return processed;
    }
}

public class ExogenousNarrativeAggregationJob
{
    private readonly ExogenousNarrativeAggregator _aggregator;

    public ExogenousNarrativeAggregationJob(ExogenousNarrativeAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public Task<int> RunAsync(CancellationToken ct) => _aggregator.AggregateAsync(ct);
}

public class ExogenousDecisionInputsJob
{
    private readonly ExogenousDecisionInputsPublisher _publisher;

    public ExogenousDecisionInputsJob(ExogenousDecisionInputsPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task RunAsync(DateTime timestampUtc, CancellationToken ct) => _publisher.PublishAsync(timestampUtc, ct);
}

public class ExogenousNarrativeRebuildJob
{
    private readonly ExogenousNarrativeAggregator _aggregator;

    public ExogenousNarrativeRebuildJob(ExogenousNarrativeAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public Task RunAsync(CancellationToken ct) => _aggregator.RebuildAsync(ct);
}
