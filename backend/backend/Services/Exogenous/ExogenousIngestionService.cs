using System.Security.Cryptography;
using System.Text;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;
using Serilog;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousIngestionService
{
    private readonly ExogenousSourceRegistry _sourceRegistry;
    private readonly ExogenousItemRepository _itemRepository;
    private readonly IEnumerable<IExogenousSourceClient> _sourceClients;

    public ExogenousIngestionService(
        ExogenousSourceRegistry sourceRegistry,
        ExogenousItemRepository itemRepository,
        IEnumerable<IExogenousSourceClient> sourceClients)
    {
        _sourceRegistry = sourceRegistry;
        _itemRepository = itemRepository;
        _sourceClients = sourceClients;
    }

    public async Task<int> IngestAsync(CancellationToken ct)
    {
        var sources = _sourceRegistry.GetSources();
        var totalAdded = 0;

        foreach (var source in sources)
        {
            var client = ResolveClient(source.Type);
            if (client == null)
            {
                Log.Warning("No source client for {SourceId} type {SourceType}", source.Id, source.Type);
                continue;
            }

            var since = await _itemRepository.GetLatestPublishedAtAsync(source.Id);

            IReadOnlyList<ExogenousContentItem> items;
            try
            {
                items = await client.FetchAsync(source, since, ct);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to fetch exogenous source {SourceId}", source.Id);
                continue;
            }

            var normalized = items
                .Where(item => IsAllowed(item.Url, source.AllowedDomains))
                .Select(item => NormalizeItem(source, item))
                .ToList();

            var added = await _itemRepository.AddNewItemsAsync(normalized);
            totalAdded += added;

            Log.Information(
                "Exogenous ingestion {SourceId} fetched={Fetched} added={Added}",
                source.Id,
                items.Count,
                added);
        }

        return totalAdded;
    }

    private IExogenousSourceClient? ResolveClient(ExogenousSourceType type)
    {
        return type switch
        {
            ExogenousSourceType.rss => _sourceClients.OfType<RssSourceClient>().FirstOrDefault(),
            ExogenousSourceType.curated => _sourceClients.OfType<CuratedLinkSourceClient>().FirstOrDefault(),
            _ => null
        };
    }

    private static bool IsAllowed(string url, List<string> allowedDomains)
    {
        if (allowedDomains.Count == 0)
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return allowedDomains.Any(domain => uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
    }

    private static ExogenousItemEntity NormalizeItem(ExogenousSourceDefinition source, ExogenousContentItem item)
    {
        var contentHash = ComputeHash($"{item.Title}|{item.Excerpt}|{item.Url}");
        return new ExogenousItemEntity
        {
            Id = Guid.NewGuid(),
            SourceId = source.Id,
            SourceCredibilityWeight = source.CredibilityWeight,
            Title = item.Title,
            Url = item.Url,
            PublishedAt = item.PublishedAt,
            FetchedAt = DateTime.UtcNow,
            ContentHash = contentHash,
            RawExcerpt = item.Excerpt,
            RawContent = null,
            Language = "en",
            Status = "NEW"
        };
    }

    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
