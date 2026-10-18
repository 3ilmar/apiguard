using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;
using SecureApiQualityPlatform.Web.ViewModels;
using static SecureApiQualityPlatform.Tests.Fixtures.Pt01Fixture;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
[DoNotParallelize]
public class DashboardPerformanceTests
{
    private const int WarmupCount = 3;
    private const int SampleCount = 20;
    private const double LimitMs = 2000;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly Dictionary<string, string> _attachments = new();

    public TestContext TestContext { get; set; } = null!;

    [TestCleanup]
    public void SaveAttachments()
    {
        // The evidence runner supplies a stable directory independent of adapter relocation.
        // Plain dotnet test runs still attach files through the normal MSTest results directory.
        var directory = TestContext.Properties["PT01EvidenceDirectory"] as string
            ?? Path.Combine(TestContext.TestResultsDirectory!, "pt01");

        Directory.CreateDirectory(directory);

        foreach (var (name, contents) in _attachments)
        {
            var path = Path.Combine(directory, name);
            File.WriteAllText(path, contents);
            TestContext.AddResultFile(path);
        }
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void PT01_BuildWithTwoThousandResults_UnderTwoSeconds()
    {
        // This test intentionally measures DashboardService aggregation
        // independently using a private in-memory fixture.
        var store = new InMemoryPlatformStore();

        Seed(store);

        var service = new DashboardService(store);
        var started = DateTimeOffset.UtcNow;

        double? first = null;
        var samples = new List<double>();
        var passed = false;
        string? error = null;

        try
        {
            first = MeasureBuild();

            for (var i = 0; i < WarmupCount; i++)
                _ = MeasureBuild();

            for (var i = 0; i < SampleCount; i++)
                samples.Add(MeasureBuild());

            AssertUnderLimit(first.Value, samples);

            passed = true;
        }
        catch (Exception exception)
        {
            error = exception.ToString();
            throw;
        }
        finally
        {
            SaveEvidence(
                "aggregation",
                store,
                started,
                passed,
                error,
                new
                {
                    Boundary =
                        "Stopwatch around DashboardService.Build(), including store snapshot/sort; excludes seeding, assertions, Razor, HTTP and browser",
                    FirstCallMs = first,
                    WarmupCount,
                    SamplesMs = samples,
                    WarmStatistics = Statistics(samples)
                });

            PrintSummary("aggregation", first, samples);
        }

        double MeasureBuild()
        {
            var timer = Stopwatch.StartNew();

            var model = service.Build();

            timer.Stop();

            // Correctness checks are outside the timing boundary.
            AssertModel(model);

            return timer.Elapsed.TotalMilliseconds;
        }
    }

    [TestMethod]
    [TestCategory("Performance")]
    [TestCategory("Integration")]
    public async Task PT01_DashboardHttpWithTwoThousandResults_UnderTwoSeconds()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"apiguard-pt01-{Guid.NewGuid():N}.db");

        try
        {
            // Run the real final application registrations, middleware,
            // controller, compiled Razor view and SQLite persistence.
            await using var app = Program.CreateApp(new[]
            {
                "--urls", "http://127.0.0.1:0",
                "--environment", "Testing",
                "--contentRoot", FindWebRoot(),
                "--Logging:LogLevel:Default", "Warning",
                "--ConnectionStrings:PlatformDatabase",
                $"Data Source={databasePath};Pooling=False"
            });

            var store =
                app.Services.GetRequiredService<IPlatformStore>();

            // The final application must now resolve the persistent SQLite store.
            Assert.IsInstanceOfType<SqlitePlatformStore>(store);

            // Seeding is deliberately outside the performance measurement.
            Seed(store);

            var originalFixtureHash = FixtureHash(store);

            await app.StartAsync();

            var started = DateTimeOffset.UtcNow;

            HttpSample? first = null;
            var samples = new List<HttpSample>();
            var passed = false;

            string? error = null;
            string? address = null;
            string? lastHtml = null;

            try
            {
                address = app.Services
                    .GetRequiredService<IServer>()
                    .Features
                    .Get<IServerAddressesFeature>()!
                    .Addresses
                    .Single();

                Assert.IsTrue(new Uri(address).IsLoopback);

                using var client = new HttpClient(
                    new SocketsHttpHandler
                    {
                        UseProxy = false,
                        AllowAutoRedirect = false
                    })
                {
                    BaseAddress = new Uri(address),
                    Timeout = TimeSpan.FromSeconds(30)
                };

                // Includes initial connection and first MVC/Razor request.
                first = await MeasureHttp();

                for (var i = 0; i < WarmupCount; i++)
                    _ = await MeasureHttp();

                for (var i = 0; i < SampleCount; i++)
                    samples.Add(await MeasureHttp());

                AssertModel(
                    app.Services
                        .GetRequiredService<DashboardService>()
                        .Build());

                Assert.AreEqual(
                    ResultCount,
                    store.GetResults().Count,
                    "HTTP GET must not change the fixture.");

                Assert.AreEqual(
                    originalFixtureHash,
                    FixtureHash(store),
                    "HTTP GET must preserve all fixture records.");

                AssertUnderLimit(
                    first.ElapsedMs,
                    samples
                        .Select(sample => sample.ElapsedMs)
                        .ToArray());

                passed = true;

                async Task<HttpSample> MeasureHttp()
                {
                    var timer = Stopwatch.StartNew();

                    using var response = await client.GetAsync(
                        "/",
                        HttpCompletionOption.ResponseHeadersRead);

                    var bytes =
                        await response.Content.ReadAsByteArrayAsync();

                    // Full HTML body has now been received.
                    timer.Stop();

                    var sample = new HttpSample(
                        timer.Elapsed.TotalMilliseconds,
                        (int)response.StatusCode,
                        bytes.Length);

                    var html = Encoding.UTF8.GetString(bytes);

                    lastHtml = html;

                    Assert.AreEqual(
                        HttpStatusCode.OK,
                        response.StatusCode,
                        "Redirects/errors cannot pass PT-01.");

                    Assert.AreEqual(
                        "text/html",
                        response.Content.Headers.ContentType?.MediaType);

                    StringAssert.Contains(
                        html,
                        "API Quality Dashboard");

                    StringAssert.Contains(
                        html,
                        "Executed checks</span><strong>2000</strong>");

                    StringAssert.Contains(
                        html,
                        "Passed / Failed</span><strong>1000 / 1000</strong>");

                    StringAssert.Contains(
                        html,
                        "Open defects</span><strong>48</strong>");

                    StringAssert.Contains(
                        html,
                        "High/Critical open</span><strong>24</strong>");

                    Assert.AreEqual(
                        12,
                        Regex.Matches(
                            html,
                            @"PT-01 check \d{4}")
                            .Count);

                    Assert.AreEqual(
                        8,
                        Regex.Matches(
                            html,
                            @"PT-01 defect \d{4}")
                            .Count);

                    StringAssert.Contains(
                        html,
                        "PT-01 check 1999");

                    StringAssert.Contains(
                        html,
                        "PT-01 defect 0079");

                    return sample;
                }
            }
            catch (Exception exception)
            {
                error = exception.ToString();
                throw;
            }
            finally
            {
                if (lastHtml is not null)
                {
                    SaveAttachment(
                        "pt01-dashboard.html",
                        lastHtml);
                }

                SaveEvidence(
                    "http",
                    store,
                    started,
                    passed,
                    error,
                    new
                    {
                        Boundary =
                            "HttpClient GET / through real loopback TCP/Kestrel + MVC + Razor + SQLite-backed store until complete HTML body received; excludes host startup, seeding, decoding/assertions, CSS fetch, browser layout/paint",
                        Url = address is null
                            ? null
                            : address + "/",
                        FirstRequest = first,
                        WarmupCount,
                        Samples = samples,
                        WarmStatistics = Statistics(
                            samples
                                .Select(sample => sample.ElapsedMs)
                                .ToArray()),
                        Concurrency = 1,
                        Connection =
                            "First request opens a connection; later requests reuse HttpClient/connection; no proxy or redirects"
                    });

                PrintSummary(
                    "HTTP",
                    first?.ElapsedMs,
                    samples
                        .Select(sample => sample.ElapsedMs)
                        .ToArray());

                await app.StopAsync();
            }
        }
        finally
        {
            // Pooling is disabled for this temporary SQLite database,
            // so the test can safely remove all database files afterwards.
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = databasePath + suffix;

                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    private static void AssertModel(DashboardViewModel model)
    {
        Assert.AreEqual(
            1,
            model.ApiCount);

        Assert.AreEqual(
            1,
            model.EndpointCount);

        Assert.AreEqual(
            ResultCount,
            model.ExecutedChecks);

        Assert.AreEqual(
            1000,
            model.PassedChecks);

        Assert.AreEqual(
            1000,
            model.FailedChecks);

        Assert.AreEqual(
            50.0,
            model.PassRatePercent);

        Assert.AreEqual(
            1333,
            model.TimedChecks);

        Assert.AreEqual(
            149.5,
            model.AverageResponseTimeMs);

        Assert.AreEqual(
            48,
            model.OpenDefects);

        Assert.AreEqual(
            24,
            model.OpenHighOrCriticalDefects);

        foreach (
            var severity
            in Enum.GetValues<DefectSeverity>())
        {
            Assert.AreEqual(
                12,
                model.OpenDefectsBySeverity[severity]);
        }

        Assert.AreEqual(
            12,
            model.RecentResults.Count);

        Assert.AreEqual(
            Id(11999),
            model.RecentResults[0].Id);

        Assert.AreEqual(
            8,
            model.RecentDefects.Count);

        Assert.AreEqual(
            Id(20079),
            model.RecentDefects[0].Id);
    }

    private static void AssertUnderLimit(
        double first,
        IReadOnlyCollection<double> samples)
    {
        Assert.AreEqual(
            SampleCount,
            samples.Count);

        var worst =
            Math.Max(
                first,
                samples.Max());

        Assert.IsTrue(
            worst < LimitMs,
            $"PT-01 worst first/warmed duration was {worst:F3} ms; every measured duration must be < {LimitMs} ms.");
    }

    private void SaveEvidence(
        string kind,
        IPlatformStore store,
        DateTimeOffset started,
        bool passed,
        string? error,
        object measurement)
    {
        var record = new
        {
            Test = "PT-01",
            Kind = kind,
            Status = passed ? "PASS" : "FAIL",
            Error = error,

            StartedUtc = started,
            FinishedUtc = DateTimeOffset.UtcNow,

            ThresholdMs = LimitMs,

            Rule =
                "Every first and warmed measured duration must be strictly below 2000 ms; correctness must pass",

            SampleCount,
            ResultCount,

            ApiCount = 1,
            EndpointCount = 1,
            DefectCount = 80,

            Fixture =
                $"Fixed IDs and UTC timestamps; 1000 pass/1000 fail, 1333 timings, 48 open defects (12 per severity); isolated {store.GetType().Name}; no outbound checks",

            FixtureSha256 =
                FixtureHash(store),

            Runtime =
                RuntimeInformation.FrameworkDescription,

            OS =
                RuntimeInformation.OSDescription,

            Architecture =
                RuntimeInformation.ProcessArchitecture.ToString(),

            LogicalProcessors =
                Environment.ProcessorCount,

            ProcessId =
                Environment.ProcessId,

            Stopwatch.Frequency,
            Stopwatch.IsHighResolution,

            BuildConfiguration =
                BuildConfiguration(),

            TargetFramework =
                "net8.0",

            Measurement =
                measurement
        };

        SaveAttachment(
            $"pt01-{kind}.json",
            JsonSerializer.Serialize(
                record,
                JsonOptions));
    }

    private void SaveAttachment(string name, string contents)
    {
        _attachments[name] = contents;
    }

    private void PrintSummary(
        string kind,
        double? first,
        IReadOnlyList<double> samples)
    {
        Console.WriteLine(
            $"PT-01 {kind}: first={first:F3} ms; warm statistics={JsonSerializer.Serialize(Statistics(samples))}; limit < {LimitMs} ms");

        Console.WriteLine(
            $"Warm samples (ms): {JsonSerializer.Serialize(samples)}");
    }

    private static object? Statistics(
        IReadOnlyList<double> samples)
    {
        if (samples.Count == 0)
            return null;

        var sorted =
            samples.Order().ToArray();

        var middle =
            sorted.Length / 2;

        var median =
            sorted.Length % 2 == 0
                ? (sorted[middle - 1] + sorted[middle]) / 2
                : sorted[middle];

        return new
        {
            MinMs = sorted[0],
            MedianMs = median,
            MeanMs = samples.Average(),
            P95Ms =
                sorted[
                    (int)Math.Ceiling(
                        sorted.Length * 0.95) - 1],
            MaxMs = sorted[^1]
        };
    }

    private static string FindWebRoot()
    {
        for (
            var directory =
                new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent)
        {
            var candidate =
                Path.Combine(
                    directory.FullName,
                    "SecureApiQualityPlatform.Web");

            if (
                File.Exists(
                    Path.Combine(
                        candidate,
                        "SecureApiQualityPlatform.Web.csproj")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Run PT-01 from a built repository checkout containing SecureApiQualityPlatform.Web.");
    }

    private static string BuildConfiguration()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    private sealed record HttpSample(
        double ElapsedMs,
        int StatusCode,
        int BodyBytes);
}