using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class ApiCheckServiceReliabilityTests
{
    private sealed class ConnectionRefusedHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            throw new HttpRequestException("Connection refused");
        }
    }

    private static (ApiCheckService Service, InMemoryPlatformStore Store, RegisteredApi Api, ConnectionRefusedHandler Handler)
        Arrange(int endpointCount)
    {
        // Private targets are allowed only so this test never needs a real DNS lookup.
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Safety:AllowPrivateNetworkTargets"] = "true" })
            .Build();

        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi { Name = "Reliability API", BaseUrl = "https://api.example.test" };
        store.AddApi(api);

        for (var i = 1; i <= endpointCount; i++)
        {
            store.AddEndpoint(api.Id, new ApiEndpoint { ApiId = api.Id, Name = $"ep{i}", Path = $"/ep{i}", Method = "GET" });
        }

        var handler = new ConnectionRefusedHandler();
        var service = new ApiCheckService(
            new HttpClient(handler),
            store,
            new CheckEvaluator(),
            new DefectLifecycleService(store),
            new UrlSafetyService(config));

        return (service, store, api, handler);
    }

    [TestMethod]
    public async Task UnreachableTarget_RecordsAvailabilityFailure_AndCreatesDefect()
    {
        var (service, store, api, _) = Arrange(1);

        var results = await service.RunApiAsync(api.Id);

        var availability = results.Single(r => r.CheckType == "Availability");
        Assert.IsFalse(availability.Passed);
        StringAssert.Contains(availability.Message, "Connection refused");
        Assert.AreEqual(1, store.GetDefects().Count(d => d.CheckType == "Availability"));
    }

    [TestMethod]
    public async Task OneFailingEndpoint_DoesNotStopLaterEndpoints()
    {
        var (service, store, api, handler) = Arrange(3);

        var results = await service.RunApiAsync(api.Id);

        Assert.AreEqual(3, handler.Calls);
        Assert.AreEqual(3, results.Count(r => r.CheckType == "Availability" && !r.Passed));
        Assert.AreEqual(3, store.GetResultsForApi(api.Id).Count(r => r.CheckType == "Availability"));
    }
}