using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class DefectLifecycleServiceTests
{
    [TestMethod]
    public void FailedCheck_CreatesDefectWithMappedPriority()
    {
        var store = new InMemoryPlatformStore();
        var service = new DefectLifecycleService(store);
        var endpointId = Guid.NewGuid();
        service.ApplyCheckResult(new ApiCheckResult
        {
            ApiId = Guid.NewGuid(), EndpointId = endpointId, CheckType = "Authentication protection",
            Passed = false, Message = "Unauthenticated access allowed", FailureSeverity = DefectSeverity.Critical
        });

        var defect = store.GetDefects().Single();
        Assert.AreEqual(DefectSeverity.Critical, defect.Severity);
        Assert.AreEqual(DefectPriority.Urgent, defect.Priority);
        Assert.AreEqual(DefectStatus.New, defect.Status);
    }

    [TestMethod]
    public void PassingRetest_ClosesResolvedDefect()
    {
        var store = new InMemoryPlatformStore();
        var service = new DefectLifecycleService(store);
        var apiId = Guid.NewGuid();
        var endpointId = Guid.NewGuid();

        service.ApplyCheckResult(new ApiCheckResult
        {
            ApiId = apiId, EndpointId = endpointId, CheckType = "Expected status",
            Passed = false, Message = "Wrong status", FailureSeverity = DefectSeverity.High
        });
        var defect = store.GetDefects().Single();
        defect.Status = DefectStatus.Resolved;
        store.UpdateDefect(defect);

        service.ApplyCheckResult(new ApiCheckResult
        {
            ApiId = apiId, EndpointId = endpointId, CheckType = "Expected status",
            Passed = true, Message = "Expected status returned", FailureSeverity = DefectSeverity.High
        });

        Assert.AreEqual(DefectStatus.Closed, defect.Status);
        Assert.IsNotNull(defect.RetestedAtUtc);
    }
}
