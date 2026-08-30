using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Controllers;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class ReportsControllerTests
{
    [TestMethod]
    public void ExportResultsCsv_IncludesQaContextAndEscapesCsvValues()
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi
        {
            Name = "Payments, API",
            BaseUrl = "https://example.com"
        };
        store.AddApi(api);

        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Health \"check\"",
            Method = "GET",
            Path = "/health",
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1000
        };
        store.AddEndpoint(api.Id, endpoint);

        store.AddResult(new ApiCheckResult
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            CheckType = "ExpectedStatus",
            Passed = false,
            ActualStatusCode = 500,
            ResponseTimeMs = 245,
            FailureSeverity = DefectSeverity.High,
            Message = "Expected 200, received \"500\"",
            ExecutedAtUtc = new DateTimeOffset(2026, 8, 30, 8, 15, 0, TimeSpan.Zero)
        });

        var actionResult = new ReportsController(store).ExportResultsCsv();
        var file = actionResult as FileContentResult;

        Assert.IsNotNull(file);
        Assert.AreEqual("text/csv; charset=utf-8", file.ContentType);
        Assert.IsTrue(file.FileDownloadName.StartsWith("api-check-results-", StringComparison.Ordinal));
        Assert.IsTrue(file.FileDownloadName.EndsWith(".csv", StringComparison.Ordinal));

        var csv = Encoding.UTF8.GetString(file.FileContents);
        StringAssert.Contains(csv, "ExecutedAtUtc,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Passed,ActualStatusCode,ResponseTimeMs,FailureSeverity,Message");
        StringAssert.Contains(csv, "\"Payments, API\"");
        StringAssert.Contains(csv, "\"Health \"\"check\"\"\"");
        StringAssert.Contains(csv, "\"ExpectedStatus\"");
        StringAssert.Contains(csv, "\"High\"");
        StringAssert.Contains(csv, "\"Expected 200, received \"\"500\"\"\"");
    }

    [TestMethod]
    public void ExportDefectsCsv_IncludesLifecycleEvidenceAndEscapesCsvValues()
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi
        {
            Name = "Orders API",
            BaseUrl = "https://example.com"
        };
        store.AddApi(api);

        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Create order",
            Method = "POST",
            Path = "/orders"
        };
        store.AddEndpoint(api.Id, endpoint);

        var sourceResultId = Guid.NewGuid();
        store.AddDefect(new Defect
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            SourceCheckResultId = sourceResultId,
            Title = "Quote \"handling\" defect",
            Description = "Contains, comma",
            CheckType = "ExpectedStatus",
            Severity = DefectSeverity.High,
            Priority = DefectPriority.Urgent,
            Status = DefectStatus.Retest,
            AssignedTo = "Shirui",
            CreatedAtUtc = new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero),
            UpdatedAtUtc = new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero),
            RetestedAtUtc = new DateTimeOffset(2026, 8, 30, 10, 5, 0, TimeSpan.Zero)
        });

        var actionResult = new ReportsController(store).ExportDefectsCsv();
        var file = actionResult as FileContentResult;

        Assert.IsNotNull(file);
        Assert.AreEqual("text/csv; charset=utf-8", file.ContentType);
        Assert.IsTrue(file.FileDownloadName.StartsWith("defects-", StringComparison.Ordinal));

        var csv = Encoding.UTF8.GetString(file.FileContents);
        StringAssert.Contains(csv, "DefectId,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Severity,Priority,Status,AssignedTo,CreatedAtUtc,UpdatedAtUtc,RetestedAtUtc,SourceCheckResultId,Title,Description");
        StringAssert.Contains(csv, "\"Orders API\"");
        StringAssert.Contains(csv, "\"POST\"");
        StringAssert.Contains(csv, "\"Retest\"");
        StringAssert.Contains(csv, sourceResultId.ToString());
        StringAssert.Contains(csv, "\"Quote \"\"handling\"\" defect\"");
        StringAssert.Contains(csv, "\"Contains, comma\"");
    }

    [TestMethod]
    public void ExportResultsCsv_WithNoResults_ReturnsHeaderOnly()
    {
        var actionResult = new ReportsController(new InMemoryPlatformStore()).ExportResultsCsv();
        var file = actionResult as FileContentResult;

        Assert.IsNotNull(file);

        var csv = Encoding.UTF8.GetString(file.FileContents);
        var lines = csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual(1, lines.Length);
        StringAssert.StartsWith(lines[0], "ExecutedAtUtc,ApiName,ApiId");
    }
}
