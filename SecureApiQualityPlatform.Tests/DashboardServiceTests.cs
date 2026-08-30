using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class DashboardServiceTests
{
    [TestMethod]
    public void Build_CalculatesPassRateFromExecutedChecks()
    {
        var store = new InMemoryPlatformStore();
        store.AddResult(new ApiCheckResult { ApiId = Guid.NewGuid(), EndpointId = Guid.NewGuid(), CheckType = "A", Passed = true });
        store.AddResult(new ApiCheckResult { ApiId = Guid.NewGuid(), EndpointId = Guid.NewGuid(), CheckType = "B", Passed = false });

        var dashboard = new DashboardService(store).Build();

        Assert.AreEqual(2, dashboard.ExecutedChecks);
        Assert.AreEqual(50.0, dashboard.PassRatePercent);
    }
}
