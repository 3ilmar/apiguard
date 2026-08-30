using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.ViewModels;

public sealed class DashboardViewModel
{
    public int ApiCount { get; init; }
    public int EndpointCount { get; init; }
    public int ExecutedChecks { get; init; }
    public int PassedChecks { get; init; }
    public double PassRatePercent { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public int OpenDefects { get; init; }
    public int OpenHighOrCriticalDefects { get; init; }
    public IReadOnlyList<ApiCheckResult> RecentResults { get; init; } = Array.Empty<ApiCheckResult>();
    public IReadOnlyList<Defect> RecentDefects { get; init; } = Array.Empty<Defect>();
}
