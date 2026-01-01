using System.Text.Json.Serialization;

namespace CryptoAgent.Api.Models.Exogenous;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousTheme
{
    AI_COMPUTE,
    ETH_ECOSYSTEM,
    NONE
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousImpactHorizon
{
    NOISE,
    TRANSITIONAL,
    STRUCTURAL
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousDirectionalBias
{
    SUPPORTIVE,
    ADVERSE,
    NEUTRAL
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousItemStatus
{
    NEW,
    CLASSIFIED,
    FAILED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousSourceType
{
    rss,
    api,
    curated
}

public class ExogenousSourceDefinition
{
    public string Id { get; set; } = string.Empty;
    public ExogenousSourceType Type { get; set; } = ExogenousSourceType.rss;
    public string Url { get; set; } = string.Empty;
    public int PollIntervalMinutes { get; set; } = 60;
    public decimal CredibilityWeight { get; set; } = 0.5m;
    public List<string> AllowedDomains { get; set; } = new();
    public ExogenousTheme? ThemeHint { get; set; }
    public List<string> CuratedLinks { get; set; } = new();
}

public class ExogenousContentItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Excerpt { get; set; } = string.Empty;
}

public class ExogenousClassifierResult
{
    public ExogenousTheme ThemeRelevance { get; set; }
    public ExogenousImpactHorizon ImpactHorizon { get; set; }
    public ExogenousDirectionalBias DirectionalBias { get; set; }
    public decimal ConfidenceScore { get; set; }
    public decimal? NoveltyScore { get; set; }
    public List<string> SummaryBullets { get; set; } = new();
    public List<string> KeyEntities { get; set; } = new();
}

public class ExogenousItemDto
{
    public Guid Id { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public decimal SourceCredibilityWeight { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime FetchedAt { get; set; }
    public string RawExcerpt { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public ExogenousItemStatus Status { get; set; }
    public string? Error { get; set; }
}

public class ExogenousClassificationDto
{
    public Guid ItemId { get; set; }
    public ExogenousTheme ThemeRelevance { get; set; }
    public ExogenousImpactHorizon ImpactHorizon { get; set; }
    public ExogenousDirectionalBias DirectionalBias { get; set; }
    public decimal ConfidenceScore { get; set; }
    public decimal? NoveltyScore { get; set; }
    public List<string> SummaryBullets { get; set; } = new();
    public List<string> KeyEntities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ExogenousNarrativeDto
{
    public Guid Id { get; set; }
    public ExogenousTheme Theme { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public decimal StateScore { get; set; }
    public ExogenousDirectionalBias DirectionalBias { get; set; }
    public ExogenousImpactHorizon Horizon { get; set; }
    public bool IsActive { get; set; }
}

public class ExogenousDecisionInputsDto
{
    public DateTime TimestampUtc { get; set; }
    public Dictionary<string, decimal> ThemeScores { get; set; } = new();
    public Dictionary<string, string> AlignmentFlags { get; set; } = new();
    public decimal AbstainModifier { get; set; }
    public decimal ConfidenceThresholdModifier { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<string> TraceIds { get; set; } = new();
}

public class ExogenousDecisionTrace
{
    public List<ExogenousNarrativeTrace> Narratives { get; set; } = new();
    public List<ExogenousItemTrace> Items { get; set; } = new();
    public decimal AbstainModifier { get; set; }
    public decimal ConfidenceThresholdModifier { get; set; }
    public List<string> WhyBullets { get; set; } = new();
}

public class ExogenousNarrativeTrace
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public ExogenousTheme Theme { get; set; }
    public decimal StateScore { get; set; }
    public ExogenousDirectionalBias DirectionalBias { get; set; }
    public ExogenousImpactHorizon Horizon { get; set; }
}

public class ExogenousItemTrace
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Url { get; set; } = string.Empty;
}
