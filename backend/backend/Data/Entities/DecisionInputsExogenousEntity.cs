namespace CryptoAgent.Api.Data.Entities;

public class DecisionInputsExogenousEntity
{
    public DateTime TimestampUtc { get; set; }
    public string ThemeScoresJson { get; set; } = "{}";
    public string ThemeStrengthJson { get; set; } = "{}";
    public string ThemeDirectionJson { get; set; } = "{}";
    public string ThemeConflictJson { get; set; } = "{}";
    public string AlignmentFlagsJson { get; set; } = "{}";
    public string MarketAlignmentJson { get; set; } = "{}";
    public string GatingReasonJson { get; set; } = "{}";
    public decimal AbstainModifier { get; set; }
    public decimal ConfidenceThresholdModifier { get; set; }
    public decimal PositionSizeModifier { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string TraceIdsJson { get; set; } = "[]";
}
