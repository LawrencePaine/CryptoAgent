namespace CryptoAgent.Api.Data.Entities;

public class NarrativeEntity
{
    public Guid Id { get; set; }
    public string Theme { get; set; } = "NONE";
    public string Label { get; set; } = string.Empty;
    public string SeedText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public decimal StateScore { get; set; }
    public string DirectionalBias { get; set; } = "NEUTRAL";
    public string Horizon { get; set; } = "NOISE";
    public bool IsActive { get; set; }
}
