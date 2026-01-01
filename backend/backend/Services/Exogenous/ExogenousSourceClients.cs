using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CryptoAgent.Api.Models.Exogenous;

namespace CryptoAgent.Api.Services.Exogenous;

public interface IExogenousSourceClient
{
    Task<IReadOnlyList<ExogenousContentItem>> FetchAsync(ExogenousSourceDefinition source, DateTime? sinceUtc, CancellationToken ct);
}

public class RssSourceClient : IExogenousSourceClient
{
    private static readonly Regex HtmlRegex = new("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline);
    private readonly IHttpClientFactory _httpClientFactory;

    public RssSourceClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ExogenousContentItem>> FetchAsync(ExogenousSourceDefinition source, DateTime? sinceUtc, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("exogenous");
        var response = await client.GetAsync(source.Url, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(ct);
        var doc = XDocument.Parse(payload);

        var entries = doc.Descendants("item").ToList();
        if (entries.Count == 0)
        {
            entries = doc.Descendants().Where(x => x.Name.LocalName == "entry").ToList();
        }

        var items = new List<ExogenousContentItem>();
        foreach (var entry in entries)
        {
            var title = GetValue(entry, "title");
            var link = GetLink(entry);
            var published = GetDate(entry);
            var excerpt = GetValue(entry, "description")
                          ?? GetValue(entry, "summary")
                          ?? GetValue(entry, "content")
                          ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            {
                continue;
            }

            if (sinceUtc.HasValue && published <= sinceUtc.Value)
            {
                continue;
            }

            var sanitizedExcerpt = HtmlRegex.Replace(excerpt, " ").Trim();

            items.Add(new ExogenousContentItem
            {
                Title = title.Trim(),
                Url = link.Trim(),
                PublishedAt = published,
                Excerpt = sanitizedExcerpt
            });
        }

        return items;
    }

    private static string? GetValue(XElement entry, string elementName)
    {
        return entry.Elements().FirstOrDefault(e => e.Name.LocalName == elementName)?.Value;
    }

    private static string GetLink(XElement entry)
    {
        var linkElement = entry.Elements().FirstOrDefault(e => e.Name.LocalName == "link");
        if (linkElement == null)
        {
            return string.Empty;
        }

        var href = linkElement.Attribute("href")?.Value;
        return string.IsNullOrWhiteSpace(href) ? linkElement.Value : href;
    }

    private static DateTime GetDate(XElement entry)
    {
        var dateRaw = GetValue(entry, "pubDate")
                      ?? GetValue(entry, "published")
                      ?? GetValue(entry, "updated")
                      ?? string.Empty;

        if (DateTime.TryParse(dateRaw, out var parsed))
        {
            return DateTime.SpecifyKind(parsed.ToUniversalTime(), DateTimeKind.Utc);
        }

        return DateTime.UtcNow;
    }
}

public class CuratedLinkSourceClient : IExogenousSourceClient
{
    public Task<IReadOnlyList<ExogenousContentItem>> FetchAsync(ExogenousSourceDefinition source, DateTime? sinceUtc, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var items = source.CuratedLinks
            .Select(link => new ExogenousContentItem
            {
                Title = link,
                Url = link,
                PublishedAt = now,
                Excerpt = "Curated link"
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<ExogenousContentItem>>(items);
    }
}
