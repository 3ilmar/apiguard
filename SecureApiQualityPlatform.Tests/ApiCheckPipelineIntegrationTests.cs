using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Controllers;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class ApiCheckPipelineIntegrationTests
{
    [DataTestMethod]
    [TestCategory("Integration")]
    [DataRow(DefectStatus.Resolved)]
    [DataRow(DefectStatus.Retest)]
    public async Task OwnedLoopbackFixture_FailureAndPassingRetest_ReachDashboardAndReports(DefectStatus verificationStatus)
    {
        var statusCode = StatusCodes.Status503ServiceUnavailable;
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
        await using var fixture = builder.Build();
        fixture.MapGet("/health", () => Results.Json(new { ready = true }, statusCode: Volatile.Read(ref statusCode)));
        await fixture.StartAsync();
        try
        {
            var addresses = fixture.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            Assert.IsNotNull(addresses);
            var baseUrl = addresses.Addresses.Single();
            Assert.IsTrue(new Uri(baseUrl).IsLoopback);

            var store = new InMemoryPlatformStore();
            var (api, endpoint) = RegisterFixture(store, baseUrl);
            using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false, AllowAutoRedirect = false })
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var checks = CreateChecks(store, client);

            var firstRun = await checks.RunApiAsync(api.Id);
            var statusFailure = firstRun.Single(result => result.CheckType == "Expected status");
            Assert.IsFalse(statusFailure.Passed);
            Assert.AreEqual(503, statusFailure.ActualStatusCode);
            var defect = store.GetDefects().Single(item => item.CheckType == "Expected status");
            Assert.AreEqual(DefectStatus.New, defect.Status);
            Assert.AreEqual(statusFailure.Id, defect.SourceCheckResultId);

            // The owned fixture deliberately uses HTTP without a test certificate.
            // HTTPS transport therefore fails on both runs and remains an open defect.
            Assert.IsFalse(firstRun.Single(result => result.CheckType == "HTTPS transport").Passed);
            Console.WriteLine($"Owned loopback fixture: {baseUrl}; HTTP intentionally fails HTTPS transport. Retest from {verificationStatus}.");

            defect.Status = verificationStatus;
            store.UpdateDefect(defect);
            Volatile.Write(ref statusCode, StatusCodes.Status200OK);
            var secondRun = await checks.RunApiAsync(api.Id);

            Assert.IsTrue(secondRun.Single(result => result.CheckType == "Expected status").Passed);
            Assert.AreEqual(DefectStatus.Closed, defect.Status);
            Assert.IsNotNull(defect.RetestedAtUtc);
            Assert.AreEqual(2, store.GetDefects().Count);
            Assert.AreEqual(firstRun.Count + secondRun.Count, store.GetResults().Count);

            var dashboard = new DashboardService(store).Build();
            Assert.AreEqual(1, dashboard.ApiCount);
            Assert.AreEqual(1, dashboard.EndpointCount);
            Assert.AreEqual(12, dashboard.ExecutedChecks);
            Assert.AreEqual(9, dashboard.PassedChecks);
            Assert.AreEqual(3, dashboard.FailedChecks);
            Assert.AreEqual(75.0, dashboard.PassRatePercent);
            Assert.AreEqual(1, dashboard.OpenDefects);
            Assert.AreEqual(1, dashboard.OpenDefectsBySeverity[DefectSeverity.High]);
            Assert.AreEqual(1, dashboard.OpenHighOrCriticalDefects);

            var reports = new ReportsController(store);
            var closedCsv = ReadCsv(reports.ExportDefectsCsv(status: DefectStatus.Closed, apiId: api.Id));
            Assert.AreEqual(2, CsvLineCount(closedCsv));
            StringAssert.Contains(closedCsv, $"\"{defect.Id}\"");
            StringAssert.Contains(closedCsv, "\"Expected status\"");
            StringAssert.Contains(closedCsv, "\"Closed\",\"False\"");
            StringAssert.Contains(closedCsv, defect.RetestedAtUtc.Value.ToString("O"));

            var failedCsv = ReadCsv(reports.ExportResultsCsv(api.Id, passed: false));
            Assert.AreEqual(4, CsvLineCount(failedCsv));
            StringAssert.Contains(failedCsv, "\"Expected HTTP 200 but received 503.\"");
            StringAssert.Contains(failedCsv, "\"HTTPS transport\"");
            StringAssert.Contains(failedCsv, $"\"{endpoint.Id}\"");
            StringAssert.Contains(failedCsv, $"\"{baseUrl}\"");
        }
        finally
        {
            await fixture.StopAsync();
        }
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Simulation")]
    public async Task SimulatedTransportTimeout_RecordsFailureAndAllowsNextCheckRun()
    {
        var store = new InMemoryPlatformStore();
        var (api, _) = RegisterFixture(store, "http://127.0.0.1");
        using var handler = new TimeoutThenSuccessHandler();
        using var client = new HttpClient(handler);
        var checks = CreateChecks(store, client);

        var firstRun = await checks.RunApiAsync(api.Id);
        var timeout = firstRun.Single(result => result.CheckType == "Availability");
        Assert.IsFalse(timeout.Passed);
        StringAssert.Contains(timeout.Message, "Request timed out.");
        Assert.AreEqual(1, store.GetDefects().Count(defect => defect.CheckType == "Availability"));

        var secondRun = await checks.RunApiAsync(api.Id);

        Assert.AreEqual(2, handler.RequestCount);
        Assert.IsTrue(secondRun.Single(result => result.CheckType == "Availability").Passed);
        Assert.IsTrue(secondRun.Single(result => result.CheckType == "Expected status").Passed);
        Assert.AreEqual(firstRun.Count + secondRun.Count, store.GetResults().Count);
        Console.WriteLine("Deterministic timeout simulation: HttpMessageHandler throws TaskCanceledException, then returns HTTP 200; no network request or elapsed timeout is measured. HTTP intentionally fails HTTPS transport.");
    }

    private static (RegisteredApi Api, ApiEndpoint Endpoint) RegisterFixture(InMemoryPlatformStore store, string baseUrl)
    {
        var api = new RegisteredApi { Name = "Owned loopback fixture", BaseUrl = baseUrl };
        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Fixture health",
            Path = "/health",
            ExpectedStatusCode = 200,
            ExpectedContentType = "application/json",
            MaxResponseTimeMs = 60000
        };
        store.AddApi(api);
        store.AddEndpoint(api.Id, endpoint);
        return (api, endpoint);
    }

    private static ApiCheckService CreateChecks(InMemoryPlatformStore store, HttpClient client)
    {
        // Test-only override for this owned loopback fixture; production defaults stay intact.
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Safety:AllowPrivateNetworkTargets"] = "true"
        }).Build();
        return new ApiCheckService(client, store, new CheckEvaluator(),
            new DefectLifecycleService(store), new UrlSafetyService(configuration));
    }

    private static string ReadCsv(IActionResult result)
    {
        var file = result as FileContentResult;
        Assert.IsNotNull(file);
        return Encoding.UTF8.GetString(file.FileContents);
    }

    // The owned fixture's messages contain no embedded newlines; escaping itself
    // is covered by the dedicated CSV structure tests.
    private static int CsvLineCount(string csv) => csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;

    private sealed class TimeoutThenSuccessHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.IsNotNull(request.RequestUri);
            Assert.IsTrue(request.RequestUri.IsLoopback);
            RequestCount++;
            if (RequestCount == 1)
                throw new TaskCanceledException("Simulated timeout; no connection was attempted.");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        }
    }
}
