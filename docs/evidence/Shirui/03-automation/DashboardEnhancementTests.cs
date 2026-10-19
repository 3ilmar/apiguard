using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class DashboardEnhancementTests
{
    [TestMethod]
    public void Build_OpenSeverityDistribution_ExcludesClosedAndDeferred()
    {
        var store = new InMemoryPlatformStore();
        AddDefect(store, DefectSeverity.Low, DefectStatus.New);
        AddDefect(store, DefectSeverity.High, DefectStatus.Assigned);
        AddDefect(store, DefectSeverity.Critical, DefectStatus.Retest);
        AddDefect(store, DefectSeverity.High, DefectStatus.Closed);
        AddDefect(store, DefectSeverity.Critical, DefectStatus.Deferred);

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(3, dashboard.OpenDefects);
        Assert.AreEqual(2, dashboard.OpenHighOrCriticalDefects);
        Assert.AreEqual(1, dashboard.OpenDefectsBySeverity[DefectSeverity.Low]);
        Assert.AreEqual(0, dashboard.OpenDefectsBySeverity[DefectSeverity.Medium]);
        Assert.AreEqual(1, dashboard.OpenDefectsBySeverity[DefectSeverity.High]);
        Assert.AreEqual(1, dashboard.OpenDefectsBySeverity[DefectSeverity.Critical]);
    }

    [TestMethod]
    public void Build_NoDefects_StillProvidesAllSeverityLabels()
    {
        var dashboard = new DashboardService(new InMemoryPlatformStore()).Build();

        Assert.AreEqual(0, dashboard.OpenDefects);
        Assert.AreEqual(4, dashboard.OpenDefectsBySeverity.Count);
        foreach (var severity in Enum.GetValues<DefectSeverity>())
            Assert.AreEqual(0, dashboard.OpenDefectsBySeverity[severity]);
    }

    [TestMethod]
    public void Build_UnorderedResults_StillReturnsLatestTwelve()
    {
        var results = new List<ApiCheckResult>();
        var origin = new DateTimeOffset(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
        // Intentionally insert out of order to test bounded selection.
        foreach (var index in new[] { 12, 0, 9, 3, 1, 11, 6, 4, 13, 2, 8, 5, 7, 10 })
        {
            results.Add(new ApiCheckResult
            {
                ApiId = Guid.NewGuid(),
                EndpointId = Guid.NewGuid(),
                CheckType = $"Check-{index}",
                Passed = true,
                ExecutedAtUtc = origin.AddMinutes(index)
            });
        }

        var dashboard = new DashboardService(new SnapshotStore(results)).Build();

        Assert.AreEqual(12, dashboard.RecentResults.Count);
        Assert.AreEqual("Check-13", dashboard.RecentResults[0].CheckType);
        Assert.AreEqual("Check-2", dashboard.RecentResults[^1].CheckType);
    }

    [TestMethod]
    public void Build_TiedResults_PreservesSnapshotOrderAtDisplayLimit()
    {
        var origin = new DateTimeOffset(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
        var results = Enumerable.Range(0, 14).Select(index => new ApiCheckResult
        {
            CheckType = $"Check-{index}",
            ExecutedAtUtc = origin
        }).ToList();
        // A late newer result must evict the last tied item, not an arbitrary tie.
        results.Add(new ApiCheckResult { CheckType = "Newest", ExecutedAtUtc = origin.AddMinutes(1) });

        var dashboard = new DashboardService(new SnapshotStore(results)).Build();

        CollectionAssert.AreEqual(
            new[] { "Newest", "Check-0", "Check-1", "Check-2", "Check-3", "Check-4", "Check-5",
                "Check-6", "Check-7", "Check-8", "Check-9", "Check-10" },
            dashboard.RecentResults.Select(result => result.CheckType).ToArray());
    }

    [TestMethod]
    public void Build_UnorderedAndTiedDefects_PreservesSnapshotOrderAtDisplayLimit()
    {
        var origin = new DateTimeOffset(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);
        var defects = Enumerable.Range(0, 10).Select(index => new Defect
        {
            Title = $"Defect-{index}",
            UpdatedAtUtc = origin
        }).ToList();
        defects.Insert(3, new Defect { Title = "Older", UpdatedAtUtc = origin.AddMinutes(-1) });
        defects.Add(new Defect { Title = "Newest", UpdatedAtUtc = origin.AddMinutes(1) });

        var dashboard = new DashboardService(new SnapshotStore(defects: defects)).Build();

        CollectionAssert.AreEqual(
            new[] { "Newest", "Defect-0", "Defect-1", "Defect-2", "Defect-3", "Defect-4", "Defect-5", "Defect-6" },
            dashboard.RecentDefects.Select(defect => defect.Title).ToArray());
    }

    [TestMethod]
    public void Build_ResultsWithoutTimings_StillCalculatesPassRate()
    {
        var results = new[]
        {
            new ApiCheckResult { Passed = true, ResponseTimeMs = null },
            new ApiCheckResult { Passed = false, ResponseTimeMs = null }
        };

        var dashboard = new DashboardService(new SnapshotStore(results)).Build();

        Assert.AreEqual(2, dashboard.ExecutedChecks);
        Assert.AreEqual(50.0, dashboard.PassRatePercent);
        Assert.AreEqual(0, dashboard.TimedChecks);
        Assert.AreEqual(0.0, dashboard.AverageResponseTimeMs);
    }

    [TestMethod]
    public void Build_OnlyClosedAndDeferredDefects_HasNoOpenSeverityCounts()
    {
        var store = new InMemoryPlatformStore();
        AddDefect(store, DefectSeverity.High, DefectStatus.Closed);
        AddDefect(store, DefectSeverity.Critical, DefectStatus.Deferred);

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(0, dashboard.OpenDefects);
        Assert.AreEqual(0, dashboard.OpenHighOrCriticalDefects);
        Assert.IsTrue(dashboard.OpenDefectsBySeverity.Values.All(count => count == 0));
        Assert.AreEqual(2, dashboard.RecentDefects.Count);
    }

    private static void AddDefect(InMemoryPlatformStore store, DefectSeverity severity, DefectStatus status)
    {
        store.AddDefect(new Defect
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            Title = $"{severity}-{status}",
            Description = "Fixture",
            Severity = severity,
            Priority = DefectPriority.Medium,
            Status = status
        });
    }

    // Return fixtures in their supplied order so the service, rather than the store,
    // is responsible for selecting and ordering recent items.
    private sealed class SnapshotStore : IPlatformStore
    {
        private readonly IReadOnlyList<ApiCheckResult> _results;
        private readonly IReadOnlyList<Defect> _defects;

        public SnapshotStore(IReadOnlyList<ApiCheckResult>? results = null, IReadOnlyList<Defect>? defects = null)
        {
            _results = results ?? Array.Empty<ApiCheckResult>();
            _defects = defects ?? Array.Empty<Defect>();
        }

        public IReadOnlyList<RegisteredApi> GetApis() => Array.Empty<RegisteredApi>();
        public RegisteredApi? GetApi(Guid id) => null;
        public IReadOnlyList<ApiCheckResult> GetResults() => _results;
        public IReadOnlyList<ApiCheckResult> GetResultsForApi(Guid apiId) => _results.Where(result => result.ApiId == apiId).ToArray();
        public IReadOnlyList<Defect> GetDefects() => _defects;
        public Defect? GetDefect(Guid id) => _defects.FirstOrDefault(defect => defect.Id == id);
        public Defect? FindOpenDefect(Guid endpointId, string checkType) => throw new NotSupportedException();
        public void AddApi(RegisteredApi api) => throw new NotSupportedException();
        public bool TryAddApi(RegisteredApi api) => throw new NotSupportedException();
        public void AddEndpoint(Guid apiId, ApiEndpoint endpoint) => throw new NotSupportedException();
        public bool TryAddEndpoint(Guid apiId, ApiEndpoint endpoint) => throw new NotSupportedException();
        public void AddResult(ApiCheckResult result) => throw new NotSupportedException();
        public void AddDefect(Defect defect) => throw new NotSupportedException();
        public void UpdateDefect(Defect defect) => throw new NotSupportedException();
    }
}
