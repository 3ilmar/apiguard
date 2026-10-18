using System.Security.Cryptography;
using System.Text.Json;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests.Fixtures;

// Shared by automated PT-01 tests and the separate development-only browser host.
// This file is not compiled into the production web application.
internal static class Pt01Fixture
{
    public const int ResultCount = 2000;
    private static readonly DateTimeOffset Origin = new(2026, 10, 18, 0, 0, 0, TimeSpan.Zero);

    public static void Seed(IPlatformStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (store.GetApis().Count != 0 || store.GetResults().Count != 0 || store.GetDefects().Count != 0)
            throw new InvalidOperationException("PT-01 requires a new isolated empty store; existing records must be preserved.");

        var api = new RegisteredApi
        {
            Id = Id(1), Name = "PT-01 isolated fixture", BaseUrl = "https://pt01.invalid",
            CreatedAtUtc = Origin
        };
        var endpoint = new ApiEndpoint { Id = Id(2), ApiId = api.Id, Name = "PT-01 recorded endpoint", Path = "/health" };
        store.AddApi(api);
        store.AddEndpoint(api.Id, endpoint);
        for (var index = 0; index < ResultCount; index++)
        {
            store.AddResult(new ApiCheckResult
            {
                Id = Id(10000 + index), ApiId = api.Id, EndpointId = endpoint.Id,
                CheckType = "Availability", Message = $"PT-01 check {index:D4}",
                Passed = index % 2 == 0, ActualStatusCode = index % 2 == 0 ? 200 : 503,
                ResponseTimeMs = index % 3 == 0 ? null : 100 + index % 100,
                ExecutedAtUtc = Origin.AddSeconds(index)
            });
        }

        var statuses = new[] { DefectStatus.New, DefectStatus.InProgress, DefectStatus.Resolved, DefectStatus.Closed, DefectStatus.Deferred };
        for (var index = 0; index < 80; index++)
        {
            store.AddDefect(new Defect
            {
                Id = Id(20000 + index), ApiId = api.Id, EndpointId = endpoint.Id,
                SourceCheckResultId = Id(10000 + index * 2 + 1),
                Title = $"PT-01 defect {index:D4}", Description = "Synthetic recorded failure; no network check executed.",
                CheckType = "Availability", Severity = (DefectSeverity)(index % 4),
                Priority = DefectPriority.Medium, Status = statuses[index % statuses.Length],
                CreatedAtUtc = Origin.AddSeconds(index), UpdatedAtUtc = Origin.AddSeconds(index)
            });
        }
    }

    public static Guid Id(int value) => new(value, 0, 0, new byte[8]);

    public static string FixtureHash(IPlatformStore store) => Convert.ToHexString(SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(new { Apis = store.GetApis(), Results = store.GetResults(), Defects = store.GetDefects() })));
}
