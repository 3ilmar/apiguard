using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class DefectLifecycleService
{
    private readonly IPlatformStore _store;

    public DefectLifecycleService(IPlatformStore store) => _store = store;

    public void ApplyCheckResult(ApiCheckResult result)
    {
        var existing = _store.FindOpenDefect(result.EndpointId, result.CheckType);

        if (!result.Passed)
        {
            if (existing is null)
            {
                _store.AddDefect(new Defect
                {
                    ApiId = result.ApiId,
                    EndpointId = result.EndpointId,
                    SourceCheckResultId = result.Id,
                    CheckType = result.CheckType,
                    Title = $"{result.CheckType} check failed",
                    Description = result.Message,
                    Severity = result.FailureSeverity,
                    Priority = MapPriority(result.FailureSeverity),
                    Status = DefectStatus.New
                });
                return;
            }

            existing.SourceCheckResultId = result.Id;
            existing.Description = result.Message;
            existing.Severity = result.FailureSeverity;
            existing.Priority = MapPriority(result.FailureSeverity);
            if (existing.Status is (DefectStatus.Resolved or DefectStatus.Retest))
                existing.Status = DefectStatus.Reopened;
            _store.UpdateDefect(existing);
            return;
        }

        if (existing is not null && existing.Status is (DefectStatus.Resolved or DefectStatus.Retest or DefectStatus.Reopened))
        {
            existing.Status = DefectStatus.Closed;
            existing.RetestedAtUtc = DateTimeOffset.UtcNow;
            _store.UpdateDefect(existing);
        }
    }

    public static DefectPriority MapPriority(DefectSeverity severity) => severity switch
    {
        DefectSeverity.Critical => DefectPriority.Urgent,
        DefectSeverity.High => DefectPriority.High,
        DefectSeverity.Medium => DefectPriority.Medium,
        _ => DefectPriority.Low
    };
}
