using System.Net;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace Sqa19.ManualHost;

// Separate executable: the normal application never loads these fixtures.
internal static class Sqa19Host
{
    private const string DashboardUrl = "http://localhost:51919";

    public static async Task Main(string[] args)
    {
        if (args.Length != 1 || !File.Exists(Path.Combine(args[0], "SecureApiQualityPlatform.Web.csproj")))
            throw new ArgumentException("Supply the original Web project directory as the only argument.");

        await using var app = global::Program.CreateApp([
            "--environment", "Development",
            "--urls", DashboardUrl,
            "--contentRoot", Path.GetFullPath(args[0])
        ]);
        if (!app.Environment.IsDevelopment())
            throw new InvalidOperationException("SQA-19 fixtures require the Development environment.");

        // This loopback-only manual session uses HTTP without certificate setup.
        app.Services.GetRequiredService<IOptions<HttpsRedirectionOptions>>().Value.HttpsPort = -1;
        var kestrel = app.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;
        kestrel.Configure(new ConfigurationBuilder().Build());
        kestrel.ListenLocalhost(51919);

        var store = app.Services.GetRequiredService<IPlatformStore>();
        if (store.GetApis().Count != 0 || store.GetResults().Count != 0 || store.GetDefects().Count != 0)
            throw new InvalidOperationException("Seed only an empty, process-local store.");

        var evaluator = app.Services.GetRequiredService<CheckEvaluator>();
        var api = new RegisteredApi
        {
            Name = "SQA-19 synthetic examples",
            BaseUrl = "https://sqa19.invalid",
            Description = "Synthetic message fixtures; no requests have been made."
        };
        store.AddApi(api);
        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "SQA-19 fixture",
            Path = "/fixture",
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1000,
            RequiresAuthentication = true,
            ExpectedContentType = "application/json"
        };
        store.AddEndpoint(api.Id, endpoint);

        // These are synthetic ordering timestamps, not measured API execution times.
        // Keep the evaluator's message and observed values intact.
        var seededAtUtc = DateTimeOffset.UtcNow;
        var results = new[]
        {
            evaluator.ExpectedStatus(api.Id, endpoint, HttpStatusCode.BadRequest),
            evaluator.ResponseTime(api.Id, endpoint, 1001),
            evaluator.AuthenticationProtection(api.Id, endpoint, HttpStatusCode.OK),
            evaluator.ContentType(api.Id, endpoint, "text/html"),
            evaluator.AvailabilityFailure(api.Id, endpoint, "Request timed out.")
        };
        for (var index = 0; index < results.Length; index++)
        {
            var result = results[index];
            store.AddResult(new ApiCheckResult
            {
                Id = result.Id,
                ApiId = result.ApiId,
                EndpointId = result.EndpointId,
                CheckType = result.CheckType,
                Passed = result.Passed,
                Message = result.Message,
                ActualStatusCode = result.ActualStatusCode,
                ResponseTimeMs = result.ResponseTimeMs,
                FailureSeverity = result.FailureSeverity,
                ExecutedAtUtc = seededAtUtc.AddSeconds(-index)
            });
        }

        // Restrict this host to reading the dashboard and its original CSS.
        // In particular, no controller action can register data or run API checks.
        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = "GET, HEAD";
                return;
            }
            var path = context.Request.Path.Value ?? "/";
            if (path != "/" && !path.Equals("/Home/Index", StringComparison.OrdinalIgnoreCase)
                && !path.Equals("/css/site.css", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            await next(context);
        });

        app.Logger.LogInformation("SQA-19 preparation only: five synthetic failures; human usability NOT RUN. Dashboard: {Url}/", DashboardUrl);
        await app.RunAsync();
    }
}
