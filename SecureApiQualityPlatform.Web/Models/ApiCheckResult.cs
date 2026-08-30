namespace SecureApiQualityPlatform.Web.Models;

public sealed class ApiCheckResult
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ApiId { get; init; }
    public Guid EndpointId { get; init; }
    public string CheckType { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Message { get; init; } = string.Empty;
    public int? ActualStatusCode { get; init; }
    public long? ResponseTimeMs { get; init; }
    public DefectSeverity FailureSeverity { get; init; } = DefectSeverity.Medium;
    public DateTimeOffset ExecutedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
