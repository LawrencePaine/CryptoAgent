namespace CryptoAgent.Api.Data.Entities;

public class NarrativeItemEntity
{
    public Guid NarrativeId { get; set; }
    public Guid ItemId { get; set; }
    public decimal ContributionWeight { get; set; }
    public DateTime AddedAt { get; set; }
}
