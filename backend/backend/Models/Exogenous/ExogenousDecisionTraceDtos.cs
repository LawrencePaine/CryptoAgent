namespace CryptoAgent.Api.Models.Exogenous;

public class ExogenousDecisionTraceDto
{
    public DateTime TickUtc { get; set; }
    public List<ThemeSummaryDto> Themes { get; set; } = new();
    public Dictionary<string, string> MarketAlignment { get; set; } = new();
    public ModifiersDto Modifiers { get; set; } = new();
    public List<string> GatingReasons { get; set; } = new();
    public List<NarrativeDto> TopNarratives { get; set; } = new();
    public List<ItemDto> TopItems { get; set; } = new();
}

public class ThemeSummaryDto
{
    public string Theme { get; set; } = string.Empty;
    public int Strength { get; set; }
    public string Direction { get; set; } = string.Empty;
    public double Conflict { get; set; }
}

public class ModifiersDto
{
    public double AbstainModifier { get; set; }
    public double ConfidenceThresholdModifier { get; set; }
    public double? PositionSizeModifier { get; set; }
}

public class NarrativeDto
{
    public string Id { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int StateScore { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Horizon { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
    public int ItemCount { get; set; }
}

public class ItemDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime PublishedAtUtc { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string ImpactHorizon { get; set; } = string.Empty;
    public string DirectionalBias { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public double? ContributionWeight { get; set; }
}
