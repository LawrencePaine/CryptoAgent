using System.Text.Json.Serialization;

namespace CryptoAgent.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousRefreshStatus
{
    Queued,
    Running,
    Succeeded,
    Failed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExogenousRefreshStartStatus
{
    Enqueued,
    AlreadyRunning,
    Cooldown
}

public class ExogenousRefreshResultSummary
{
    public DateTime TickUtc { get; set; }
    public int ItemsIngested { get; set; }
    public int ItemsClassified { get; set; }
    public int NarrativesAggregated { get; set; }
    public long DurationMs { get; set; }
}

public class ExogenousRefreshJobStatus
{
    public string JobId { get; set; } = string.Empty;
    public ExogenousRefreshStatus Status { get; set; }
    public DateTime? QueuedUtc { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? FinishedUtc { get; set; }
    public ExogenousRefreshResultSummary? ResultSummary { get; set; }
    public string? Error { get; set; }
}
