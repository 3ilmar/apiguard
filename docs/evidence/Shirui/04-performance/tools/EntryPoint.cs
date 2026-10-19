using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SecureApiQualityPlatform.Tests.Fixtures;
using SecureApiQualityPlatform.Web.Services;

namespace Pt01BrowserHost;

internal static class EntryPoint
{
    public static async Task Main(string[] args)
    {
        // Only this separate executable links the test fixture. The Web project
        // has no reference to it, seed flag, seed endpoint, or startup seeding.
        foreach (var name in new[] { "DOTNET_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT" })
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(name), "Development", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{name} must explicitly be Development for the PT-01 browser host.");
        }

        if (args.Length != 6 || args[0] != "--port" || args[2] != "--evidence-directory" || args[4] != "--content-root"
            || !int.TryParse(args[1], out var port) || port is < 1024 or > 65535)
            throw new ArgumentException("Usage: --port <1024-65535> --evidence-directory <directory> --content-root <Web project directory>");

        var evidenceDirectory = Path.GetFullPath(args[3]);
        var webRoot = Path.GetFullPath(args[5]);
        if (!File.Exists(Path.Combine(webRoot, "SecureApiQualityPlatform.Web.csproj")))
            throw new DirectoryNotFoundException("The source Web project is required; this host is development-only.");
        var metadataPath = Path.Combine(evidenceDirectory, "instance.json");
        if (File.Exists(metadataPath)) throw new IOException("Use a fresh evidence directory for each instance.");

        var url = $"http://127.0.0.1:{port}";
        await using var app = global::Program.CreateApp(new[]
        {
            "--environment", "Development", "--urls", url,
            "--contentRoot", webRoot, "--Logging:LogLevel:Default", "Warning"
        }, services =>
        {
            services.RemoveAll<IPlatformStore>();
            services.AddSingleton<IPlatformStore, InMemoryPlatformStore>();
        });
        if (!app.Environment.IsDevelopment()
            || app.Configuration.GetSection("Kestrel:Endpoints").GetChildren().Any()
            || !string.IsNullOrEmpty(app.Configuration["https_port"])
            || !string.IsNullOrEmpty(app.Configuration["ANCM_HTTPS_PORT"]))
            throw new InvalidOperationException("PT-01 requires Development, no configured Kestrel endpoints, and no HTTPS redirect port.");

        var store = app.Services.GetRequiredService<IPlatformStore>();
        if (store is not InMemoryPlatformStore)
            throw new InvalidOperationException("PT-01 requires its own in-memory store.");
        Pt01Fixture.Seed(store); // Refuses nonempty stores before making any changes.
        var fixtureHash = Pt01Fixture.FixtureHash(store);
        var model = app.Services.GetRequiredService<DashboardService>().Build();
        if (model.ExecutedChecks != 2000 || model.PassedChecks != 1000 || model.FailedChecks != 1000
            || store.GetDefects().Count != 80 || model.OpenDefects != 48
            || model.OpenHighOrCriticalDefects != 24 || model.OpenDefectsBySeverity.Values.Any(count => count != 12))
            throw new InvalidOperationException("PT-01 fixture metrics do not match the benchmark.");

        // Keep the benchmark fixed even if someone follows an edit/run-check link.
        // This restriction applies only to the separate synthetic-data host.
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-PT01-Fixture-SHA256"] = fixtureHash;
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = "GET, HEAD";
                await context.Response.WriteAsync("PT-01 browser fixture is read-only. Use your normal application for edits.");
                return;
            }
            await next(context);
        });

        await app.StartAsync(); // Kestrel fails rather than replacing an occupied port.
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.ToArray();
            if (addresses.Length != 1 || addresses[0] != url || !new Uri(addresses[0]).IsLoopback)
                throw new InvalidOperationException("PT-01 must listen only at its requested loopback URL.");
            Directory.CreateDirectory(evidenceDirectory);
            var metadataTemporaryPath = metadataPath + ".tmp";
            await File.WriteAllTextAsync(metadataTemporaryPath, JsonSerializer.Serialize(new
            {
                Case = "SQA-18", BrowserStatus = "NOT RUN", BrowserTimings = (object?)null,
                StartedAtUtc = DateTimeOffset.UtcNow, ProcessId = Environment.ProcessId,
                Url = url + "/", Environment = app.Environment.EnvironmentName,
                Configuration = BuildConfiguration, TargetFramework = "net8.0",
                Runtime = RuntimeInformation.FrameworkDescription, OS = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                LogicalProcessors = Environment.ProcessorCount,
                FixtureSha256 = fixtureHash, ResultCount = store.GetResults().Count,
                ApiCount = model.ApiCount, EndpointCount = model.EndpointCount,
                Passed = model.PassedChecks, Failed = model.FailedChecks,
                PassRatePercent = model.PassRatePercent, model.TimedChecks, model.AverageResponseTimeMs,
                DefectCount = store.GetDefects().Count, model.OpenDefects, model.OpenHighOrCriticalDefects,
                OpenDefectsBySeverity = model.OpenDefectsBySeverity,
                RenderedRecentResults = model.RecentResults.Count, RenderedRecentDefects = model.RecentDefects.Count,
                Isolation = "Fresh process/service provider/in-memory store; no manual store or persistence access; no outbound API checks",
                HostRestrictions = "Development only; loopback only; GET/HEAD only; no seed route; not publishable",
                ExternalStylesheet = "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
            }, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(metadataTemporaryPath, metadataPath);
            Console.WriteLine($"PT-01 dashboard ready: {url}/ (PID {Environment.ProcessId}); browser SQA-18 NOT RUN.");
            await app.WaitForShutdownAsync();
        }
        finally
        {
            await app.StopAsync();
        }
    }

#if DEBUG
    private const string BuildConfiguration = "Debug";
#else
    private const string BuildConfiguration = "Release";
#endif
}
