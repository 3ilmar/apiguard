using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.ViewModels;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class DashboardService
{
    private readonly IPlatformStore _store;

    public DashboardService(IPlatformStore store) => _store = store;

    public DashboardViewModel Build()
    {
        var apis = _store.GetApis();
        var results = _store.GetResults();
        var defects = _store.GetDefects();
        var passed = results.Count(r => r.Passed);
        var timings = results.Where(r => r.ResponseTimeMs.HasValue).Select(r => (double)r.ResponseTimeMs!.Value).ToArray();
        var open = defects.Where(d => d.Status != DefectStatus.Closed && d.Status != DefectStatus.Deferred).ToArray();

        return new DashboardViewModel
        {
            ApiCount = apis.Count,
            EndpointCount = apis.Sum(a => a.Endpoints.Count),
            ExecutedChecks = results.Count,
            PassedChecks = passed,
            PassRatePercent = results.Count == 0 ? 0 : Math.Round((double)passed / results.Count * 100, 1),
            AverageResponseTimeMs = timings.Length == 0 ? 0 : Math.Round(timings.Average(), 1),
            OpenDefects = open.Length,
            OpenHighOrCriticalDefects = open.Count(d => d.Severity is DefectSeverity.High or DefectSeverity.Critical),
            RecentResults = results.Take(12).ToArray(),
            RecentDefects = defects.Take(8).ToArray()
        };
    }
}
