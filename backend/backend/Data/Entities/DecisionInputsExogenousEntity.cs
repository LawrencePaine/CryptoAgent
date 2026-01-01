namespace CryptoAgent.Api.Data.Entities;

public class DecisionInputsExogenousEntity
{
    public DateTime TimestampUtc { get; set; }
    public string ThemeScoresJson { get; set; } = "{}";
    public string AlignmentFlagsJson { get; set; } = "{}";
    public decimal AbstainModifier { get; set; }
    public decimal ConfidenceThresholdModifier { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string TraceIdsJson { get; set; } = "[]";
}
