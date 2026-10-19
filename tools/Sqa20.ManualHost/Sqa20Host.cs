using System.Net;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace Sqa20.ManualHost;

// Separate executable: reuse the real MVC app without changing its normal startup or data.
internal static class Sqa20Host
{
    public static async Task Main(string[] args)
    {
        if (args.Length != 1 || !File.Exists(Path.Combine(args[0], "SecureApiQualityPlatform.Web.csproj")))
            throw new ArgumentException("Supply the original Web project directory as the only argument.");

        await using var app = global::Program.CreateApp([
            "--environment", "Development",
            "--urls", "http://localhost:51920",
            "--contentRoot", Path.GetFullPath(args[0])
        ]);
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("SQA-20 fixtures require Development.");

        // Reset inherited Kestrel endpoint configuration, then bind only to loopback.
        app.Services.GetRequiredService<IOptions<HttpsRedirectionOptions>>().Value.HttpsPort = -1;
        var kestrel = app.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;
        kestrel.Configure(new ConfigurationBuilder().Build());
        kestrel.ListenLocalhost(51920);

        var store = app.Services.GetRequiredService<IPlatformStore>();
        if (store is not InMemoryPlatformStore || store.GetApis().Count != 0 ||
            store.GetResults().Count != 0 || store.GetDefects().Count != 0)
            throw new InvalidOperationException("Seed only an empty, process-local InMemoryPlatformStore.");

        var failedApi = new RegisteredApi
        {
            Id = Guid.Parse("20200000-0000-0000-0000-000000000001"),
            Name = "SQA-20 Orders, \"review\"",
            BaseUrl = "https://sqa20-orders.invalid",
            Description = "Synthetic accessibility fixtures. No API requests have been made."
        };
        var healthyApi = new RegisteredApi
        {
            Id = Guid.Parse("20200000-0000-0000-0000-000000000002"),
            Name = "SQA-20 Catalogue, \"healthy\"",
            BaseUrl = "https://sqa20-catalogue.invalid",
            Description = "Synthetic passing checks. Values are fixtures, not live measurements."
        };
        store.AddApi(failedApi);
        store.AddApi(healthyApi);

        var failedEndpoint = new ApiEndpoint
        {
            ApiId = failedApi.Id,
            Name = "Orders, \"manual review\" endpoint with a deliberately long descriptive label",
            Path = "/v1/orders/accessibility-and-browser-compatibility-review",
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1000,
            RequiresAuthentication = true,
            ExpectedContentType = "application/json"
        };
        var healthyEndpoint = new ApiEndpoint
        {
            ApiId = healthyApi.Id,
            Name = "Catalogue, \"passing\" endpoint",
            Path = "/v1/catalogue",
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1000,
            ExpectedContentType = "application/json"
        };
        store.AddEndpoint(failedApi.Id, failedEndpoint);
        store.AddEndpoint(healthyApi.Id, healthyEndpoint);

        var evaluator = app.Services.GetRequiredService<CheckEvaluator>();
        var lifecycle = app.Services.GetRequiredService<DefectLifecycleService>();
        ApiCheckResult[] results = [
            evaluator.AuthenticationProtection(failedApi.Id, failedEndpoint, HttpStatusCode.OK),
            evaluator.ExpectedStatus(failedApi.Id, failedEndpoint, HttpStatusCode.BadRequest),
            evaluator.ResponseTime(failedApi.Id, failedEndpoint, 1500),
            evaluator.ServerDisclosure(failedApi.Id, failedEndpoint, ["SQA-20 fixture, \"synthetic\" server"]),
            evaluator.ExpectedStatus(healthyApi.Id, healthyEndpoint, HttpStatusCode.OK),
            evaluator.ResponseTime(healthyApi.Id, healthyEndpoint, 120),
            evaluator.ContentType(healthyApi.Id, healthyEndpoint, "application/json"),
            evaluator.ServerDisclosure(healthyApi.Id, healthyEndpoint, [])
        ];
        foreach (var result in results)
        {
            store.AddResult(result);
            lifecycle.ApplyCheckResult(result);
        }
        foreach (var defect in store.GetDefects())
        {
            defect.Status = defect.Severity switch
            {
                DefectSeverity.High => DefectStatus.Assigned,
                DefectSeverity.Medium => DefectStatus.InProgress,
                DefectSeverity.Low => DefectStatus.Retest,
                _ => DefectStatus.New
            };
            if (defect.Status != DefectStatus.New)
                defect.AssignedTo = "SQA-20 manual tester";
            store.UpdateDefect(defect);
        }

        // Inspection and original CSV downloads remain functional. Block every write,
        // including RunChecks, so this session cannot trigger an external API scan.
        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = "GET, HEAD";
                await context.Response.WriteAsync("SQA-20 inspection host: writes and API scans are disabled. Use the existing seeded data.");
                return;
            }
            await next(context);
        });

        app.Logger.LogInformation("SQA-20 preparation: 2 APIs, 2 endpoints, 8 synthetic checks (4 passing/4 failing), 4 open defects. Human SQA-20 NOT RUN. http://localhost:51920/");
        await app.RunAsync();
    }
}
