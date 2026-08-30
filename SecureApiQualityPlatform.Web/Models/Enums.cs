namespace SecureApiQualityPlatform.Web.Models;

public enum DefectSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum DefectPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum DefectStatus
{
    New,
    Triaged,
    Assigned,
    InProgress,
    Resolved,
    Retest,
    Closed,
    Reopened,
    Deferred
}
