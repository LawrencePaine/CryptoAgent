namespace CryptoAgent.Api.Data.Entities;

public class ExogenousClassificationEntity
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ThemeRelevance { get; set; } = "NONE";
    public string ImpactHorizon { get; set; } = "NOISE";
    public string DirectionalBias { get; set; } = "NEUTRAL";
    public decimal ConfidenceScore { get; set; }
    public decimal? NoveltyScore { get; set; }
    public string SummaryBulletsJson { get; set; } = "[]";
    public string KeyEntitiesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
}
