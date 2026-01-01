namespace CryptoAgent.Api.Data.Entities;

public class ExogenousItemEntity
{
    public Guid Id { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public decimal SourceCredibilityWeight { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime FetchedAt { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public string RawExcerpt { get; set; } = string.Empty;
    public string? RawContent { get; set; }
    public string Language { get; set; } = "en";
    public string Status { get; set; } = "NEW";
    public string? Error { get; set; }
}
