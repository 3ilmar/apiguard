using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class DashboardServiceTests
{
    [TestMethod]
    public void Build_CalculatesCheckAndResponseTimeMetrics()
    {
        var store = new InMemoryPlatformStore();
        store.AddResult(new ApiCheckResult
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            CheckType = "Availability",
            Passed = true,
            ResponseTimeMs = 100
        });
        store.AddResult(new ApiCheckResult
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            CheckType = "ExpectedStatus",
            Passed = false,
            ResponseTimeMs = 300
        });
        store.AddResult(new ApiCheckResult
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            CheckType = "ContentType",
            Passed = true,
            ResponseTimeMs = null
        });

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(3, dashboard.ExecutedChecks);
        Assert.AreEqual(2, dashboard.PassedChecks);
        Assert.AreEqual(1, dashboard.FailedChecks);
        Assert.AreEqual(66.7, dashboard.PassRatePercent);
        Assert.AreEqual(2, dashboard.TimedChecks);
        Assert.AreEqual(200.0, dashboard.AverageResponseTimeMs);
    }

    [TestMethod]
    public void Build_HandlesNoCheckDataWithoutInvalidMetrics()
    {
        var dashboard = new DashboardService(new InMemoryPlatformStore()).Build();

        Assert.AreEqual(0, dashboard.ExecutedChecks);
        Assert.AreEqual(0, dashboard.PassedChecks);
        Assert.AreEqual(0, dashboard.FailedChecks);
        Assert.AreEqual(0.0, dashboard.PassRatePercent);
        Assert.AreEqual(0, dashboard.TimedChecks);
        Assert.AreEqual(0.0, dashboard.AverageResponseTimeMs);
        Assert.AreEqual(0, dashboard.RecentResults.Count);
        Assert.AreEqual(0, dashboard.RecentDefects.Count);
    }

    [TestMethod]
    public void Build_CountsOnlyOpenHighOrCriticalDefectsAsHighRisk()
    {
        var store = new InMemoryPlatformStore();
        store.AddDefect(new Defect
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            Title = "Open high defect",
            Description = "High severity issue",
            Severity = DefectSeverity.High,
            Priority = DefectPriority.High,
            Status = DefectStatus.New
        });
        store.AddDefect(new Defect
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            Title = "Open low defect",
            Description = "Low severity issue",
            Severity = DefectSeverity.Low,
            Priority = DefectPriority.Low,
            Status = DefectStatus.Assigned
        });
        store.AddDefect(new Defect
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            Title = "Closed critical defect",
            Description = "Already closed",
            Severity = DefectSeverity.Critical,
            Priority = DefectPriority.Urgent,
            Status = DefectStatus.Closed
        });
        store.AddDefect(new Defect
        {
            ApiId = Guid.NewGuid(),
            EndpointId = Guid.NewGuid(),
            Title = "Deferred critical defect",
            Description = "Deferred by decision",
            Severity = DefectSeverity.Critical,
            Priority = DefectPriority.High,
            Status = DefectStatus.Deferred
        });

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(2, dashboard.OpenDefects);
        Assert.AreEqual(1, dashboard.OpenHighOrCriticalDefects);
    }

    [TestMethod]
    public void Build_ReturnsMostRecentChecksAndDefectsWithinDisplayLimits()
    {
        var store = new InMemoryPlatformStore();
        var start = new DateTimeOffset(2026, 8, 30, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < 14; i++)
        {
            store.AddResult(new ApiCheckResult
            {
                ApiId = Guid.NewGuid(),
                EndpointId = Guid.NewGuid(),
                CheckType = $"Check-{i}",
                Passed = true,
                ExecutedAtUtc = start.AddMinutes(i)
            });
        }

        for (var i = 0; i < 10; i++)
        {
            store.AddDefect(new Defect
            {
                ApiId = Guid.NewGuid(),
                EndpointId = Guid.NewGuid(),
                Title = $"Defect-{i}",
                Description = "Test defect",
                Severity = DefectSeverity.Medium,
                Priority = DefectPriority.Medium,
                Status = DefectStatus.New,
                UpdatedAtUtc = start.AddMinutes(i)
            });
        }

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(12, dashboard.RecentResults.Count);
        Assert.AreEqual("Check-13", dashboard.RecentResults[0].CheckType);
        Assert.AreEqual("Check-2", dashboard.RecentResults[^1].CheckType);

        Assert.AreEqual(8, dashboard.RecentDefects.Count);
        Assert.AreEqual("Defect-9", dashboard.RecentDefects[0].Title);
        Assert.AreEqual("Defect-2", dashboard.RecentDefects[^1].Title);
    }
}
