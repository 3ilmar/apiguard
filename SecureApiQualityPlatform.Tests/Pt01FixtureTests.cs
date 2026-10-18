using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Tests.Fixtures;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class Pt01FixtureTests
{
    [TestMethod]
    public void Seed_ReproducesExistingPt01FixtureAndLinkedFailures()
    {
        var first = new InMemoryPlatformStore();
        var second = new InMemoryPlatformStore();

        Pt01Fixture.Seed(first);
        Pt01Fixture.Seed(second);

        Assert.AreEqual("CCBE6723FB603221C3CECB95BB176BD6426E8F7611B96D3AC0565A1D7927AC18", Pt01Fixture.FixtureHash(first));
        Assert.AreEqual(Pt01Fixture.FixtureHash(first), Pt01Fixture.FixtureHash(second));
        Assert.AreEqual(2000, first.GetResults().Count);
        Assert.AreEqual(2000, first.GetResults().Select(result => result.Id).Distinct().Count());
        Assert.AreEqual(1000, first.GetResults().Count(result => result.Passed));
        Assert.AreEqual(1000, first.GetResults().Count(result => !result.Passed));
        Assert.AreEqual(80, first.GetDefects().Count);
        Assert.AreEqual(1, first.GetApis().Count);
        Assert.AreEqual(1, first.GetApis().Single().Endpoints.Count);
        foreach (var defect in first.GetDefects())
        {
            var source = first.GetResults().Single(result => result.Id == defect.SourceCheckResultId);
            Assert.IsFalse(source.Passed, "Every synthetic defect must refer to an existing failed check.");
            Assert.AreEqual(source.EndpointId, defect.EndpointId);
        }
    }

    [DataTestMethod]
    [DataRow("api")]
    [DataRow("result")]
    [DataRow("defect")]
    public void Seed_NonemptyStoreIsRejectedWithoutChangingAnyRecords(string recordType)
    {
        var store = new InMemoryPlatformStore();
        // Individual collections may contain records independently; every case must be checked
        // before even the fixture API is added.
        switch (recordType)
        {
            case "api":
                store.AddApi(new RegisteredApi { Name = "Manually recorded API", BaseUrl = "https://manual.invalid" });
                break;
            case "result":
                store.AddResult(new ApiCheckResult { Message = "Manually recorded check", Passed = true });
                break;
            case "defect":
                store.AddDefect(new Defect { Title = "Manually recorded defect", Description = "Preserve this record." });
                break;
        }
        var originalHash = Pt01Fixture.FixtureHash(store);

        Assert.ThrowsException<InvalidOperationException>(() => Pt01Fixture.Seed(store));

        Assert.AreEqual(originalHash, Pt01Fixture.FixtureHash(store));
        Assert.AreEqual(1, store.GetApis().Count + store.GetResults().Count + store.GetDefects().Count);
    }

    [TestMethod]
    public void Seed_SecondAttemptPreservesFirstFixture()
    {
        var store = new InMemoryPlatformStore();
        Pt01Fixture.Seed(store);
        var originalHash = Pt01Fixture.FixtureHash(store);

        Assert.ThrowsException<InvalidOperationException>(() => Pt01Fixture.Seed(store));

        Assert.AreEqual(originalHash, Pt01Fixture.FixtureHash(store));
        Assert.AreEqual(Pt01Fixture.ResultCount, store.GetResults().Count);
    }
}
