using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.ViewModels;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class DashboardService
{
    private readonly IPlatformStore _store;

    public DashboardService(IPlatformStore store) => _store = store;

    public DashboardViewModel Build()
    {
        // The store may sort snapshots; aggregate here without extra full-sized arrays.
        var apis = _store.GetApis();
        var results = _store.GetResults();
        var defects = _store.GetDefects();

        var passed = 0;
        var timed = 0;
        double totalResponseTimeMs = 0;

        foreach (var result in results)
        {
            if (result.Passed) passed++;
            if (result.ResponseTimeMs is long elapsed)
            {
                timed++;
                totalResponseTimeMs += elapsed;
            }
        }

        var distribution = Enum.GetValues<DefectSeverity>()
            .ToDictionary(severity => severity, _ => 0);
        var openCount = 0;
        var highRiskOpenCount = 0;

        foreach (var defect in defects)
        {
            if (defect.Status is DefectStatus.Closed or DefectStatus.Deferred)
                continue;

            openCount++;
            if (distribution.ContainsKey(defect.Severity))
                distribution[defect.Severity]++;

            if (defect.Severity is DefectSeverity.High or DefectSeverity.Critical)
                highRiskOpenCount++;
        }

        return new DashboardViewModel
        {
            ApiCount = apis.Count,
            EndpointCount = apis.Sum(api => api.Endpoints.Count),
            ExecutedChecks = results.Count,
            PassedChecks = passed,
            FailedChecks = results.Count - passed,
            PassRatePercent = results.Count == 0 ? 0 : Math.Round((double)passed / results.Count * 100, 1),
            TimedChecks = timed,
            AverageResponseTimeMs = timed == 0 ? 0 : Math.Round(totalResponseTimeMs / timed, 1),
            OpenDefects = openCount,
            OpenHighOrCriticalDefects = highRiskOpenCount,
            OpenDefectsBySeverity = distribution,
            RecentResults = Newest(results, result => result.ExecutedAtUtc, 12),
            RecentDefects = Newest(defects, defect => defect.UpdatedAtUtc, 8)
        };
    }

    private static IReadOnlyList<T> Newest<T>(
        IReadOnlyList<T> items,
        Func<T, DateTimeOffset> timestamp,
        int limit)
    {
        // Selection is O(n log limit), independently of any sorting done by the store.
        // Earlier snapshot items win timestamp ties, matching stable OrderByDescending.
        var heap = new PriorityQueue<T, (DateTimeOffset Time, int ReverseIndex)>();
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var priority = (Time: timestamp(item), ReverseIndex: -index);
            if (heap.Count < limit)
            {
                heap.Enqueue(item, priority);
            }
            else if (heap.TryPeek(out _, out var oldest) && priority.CompareTo(oldest) > 0)
            {
                heap.Dequeue();
                heap.Enqueue(item, priority);
            }
        }

        return heap.UnorderedItems
            .OrderByDescending(entry => entry.Priority)
            .Select(entry => entry.Element)
            .ToArray();
    }
}
