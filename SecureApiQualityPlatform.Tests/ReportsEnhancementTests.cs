using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Controllers;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class ReportsEnhancementTests
{
    private static readonly string[] ResultsHeader =
    [
        "ExecutedAtUtc", "ApiName", "ApiId", "EndpointName", "EndpointId", "Method", "Path",
        "CheckType", "Passed", "ActualStatusCode", "ResponseTimeMs", "FailureSeverity", "Message",
        "ApiBaseUrl", "Outcome"
    ];

    private static readonly string[] DefectsHeader =
    [
        "DefectId", "ApiName", "ApiId", "EndpointName", "EndpointId", "Method", "Path", "CheckType",
        "Severity", "Priority", "Status", "AssignedTo", "CreatedAtUtc", "UpdatedAtUtc", "RetestedAtUtc",
        "SourceCheckResultId", "Title", "Description", "ApiBaseUrl", "LifecycleStage", "IsOpen"
    ];

    [DataTestMethod]
    [DataRow(false, "selected-fail", "Fail")]
    [DataRow(true, "selected-pass", "Pass")]
    public void ExportResultsCsv_FiltersByApiAndPassStatus(bool passed, string message, string outcome)
    {
        var store = new InMemoryPlatformStore();
        var selectedApi = Guid.NewGuid();
        var otherApi = Guid.NewGuid();
        AddResult(store, selectedApi, true, "selected-pass");
        AddResult(store, selectedApi, false, "selected-fail");
        AddResult(store, otherApi, false, "other-fail");

        var rows = ParseCsv(new ReportsController(store).ExportResultsCsv(apiId: selectedApi, passed: passed));

        AssertSchema(rows, ResultsHeader);
        Assert.AreEqual(2, rows.Count); // Header + exactly one filtered result.
        Assert.AreEqual(selectedApi.ToString(), rows[1][2]);
        Assert.AreEqual(passed.ToString(), rows[1][8]);
        Assert.AreEqual(message, rows[1][12]);
        Assert.AreEqual(outcome, rows[1][14]);
    }

    [TestMethod]
    public void ExportDefectsCsv_FiltersSeverityStatusAndApi_AndAddsLifecycleFields()
    {
        var store = new InMemoryPlatformStore();
        var selectedApi = Guid.NewGuid();
        AddDefect(store, selectedApi, DefectSeverity.High, DefectStatus.Retest, "selected");
        AddDefect(store, selectedApi, DefectSeverity.Low, DefectStatus.Retest, "wrong-severity");
        AddDefect(store, selectedApi, DefectSeverity.High, DefectStatus.Closed, "wrong-status");
        AddDefect(store, Guid.NewGuid(), DefectSeverity.High, DefectStatus.Retest, "wrong-api");

        var rows = ParseCsv(new ReportsController(store).ExportDefectsCsv(
            status: DefectStatus.Retest, severity: DefectSeverity.High, apiId: selectedApi));

        AssertSchema(rows, DefectsHeader);
        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual(selectedApi.ToString(), rows[1][2]);
        Assert.AreEqual("High", rows[1][8]);
        Assert.AreEqual("Retest", rows[1][10]);
        Assert.AreEqual("selected", rows[1][16]);
        Assert.AreEqual("Verification", rows[1][19]);
        Assert.AreEqual("True", rows[1][20]);
    }

    [TestMethod]
    public void ExportResultsCsv_EscapesUntrustedSpreadsheetFormulaAndNewLine()
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi { Name = "=HYPERLINK(\"https://example.com\")", BaseUrl = "https://example.com" };
        store.AddApi(api);
        store.AddResult(new ApiCheckResult
        {
            ApiId = api.Id,
            EndpointId = Guid.NewGuid(),
            CheckType = "Availability",
            Passed = false,
            Message = "@SUM(1,2)\nsecond line"
        });

        var rows = ParseCsv(new ReportsController(store).ExportResultsCsv());
        AssertSchema(rows, ResultsHeader);
        Assert.AreEqual(2, rows.Count);
        Assert.AreEqual("'=HYPERLINK(\"https://example.com\")", rows[1][1]);
        Assert.AreEqual("'@SUM(1,2)\nsecond line", NormalizeNewLines(rows[1][12]));
    }

    [TestMethod]
    public void ExportDefectsCsv_WithNoMatchingRows_RemainsHeaderOnly()
    {
        var store = new InMemoryPlatformStore();
        AddDefect(store, Guid.NewGuid(), DefectSeverity.Low, DefectStatus.Closed, "closed-low");

        var rows = ParseCsv(new ReportsController(store).ExportDefectsCsv(
            status: DefectStatus.New, severity: DefectSeverity.Critical));

        AssertSchema(rows, DefectsHeader);
        Assert.AreEqual(1, rows.Count);
    }

    [TestMethod]
    public void ExportResultsCsv_PreservesSchemaAndQuotedMultilineText()
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi { Name = "Payments, \"QA\"", BaseUrl = "https://example.com" };
        store.AddApi(api);
        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Health\r\ncheck",
            Method = "GET",
            Path = "/health?name=one,two"
        };
        store.AddEndpoint(api.Id, endpoint);
        var executedAt = new DateTimeOffset(2026, 8, 30, 8, 15, 0, TimeSpan.Zero);
        store.AddResult(new ApiCheckResult
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            CheckType = "ExpectedStatus",
            Passed = false,
            ActualStatusCode = 500,
            ResponseTimeMs = 245,
            FailureSeverity = DefectSeverity.High,
            Message = "Expected 200, received \"500\"\r\nRetry failed",
            ExecutedAtUtc = executedAt
        });

        var rows = ParseCsv(new ReportsController(store).ExportResultsCsv());

        AssertSchema(rows, ResultsHeader);
        Assert.AreEqual(2, rows.Count);
        CollectionAssert.AreEqual(new[]
        {
            executedAt.ToString("O"), api.Name, api.Id.ToString(), "Health\ncheck", endpoint.Id.ToString(),
            "GET", endpoint.Path, "ExpectedStatus", "False", "500", "245", "High",
            "Expected 200, received \"500\"\nRetry failed", api.BaseUrl, "Fail"
        }, rows[1].Select(NormalizeNewLines).ToArray());
    }

    [TestMethod]
    public void ExportDefectsCsv_PreservesSchemaAndQuotedLifecycleEvidence()
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi { Name = "Orders, API", BaseUrl = "https://example.com" };
        store.AddApi(api);
        var endpoint = new ApiEndpoint { ApiId = api.Id, Name = "Create \"order\"", Method = "POST", Path = "/orders" };
        store.AddEndpoint(api.Id, endpoint);
        var createdAt = new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddDays(1);
        var retestedAt = updatedAt.AddMinutes(5);
        var sourceResultId = Guid.NewGuid();
        var defect = new Defect
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            CheckType = "ExpectedStatus",
            Severity = DefectSeverity.High,
            Priority = DefectPriority.Urgent,
            Status = DefectStatus.Retest,
            AssignedTo = "Tester, QA",
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = updatedAt,
            RetestedAtUtc = retestedAt,
            SourceCheckResultId = sourceResultId,
            Title = "Quote \"handling\" defect",
            Description = "Contains, comma\r\nsecond line"
        };
        store.AddDefect(defect);

        var rows = ParseCsv(new ReportsController(store).ExportDefectsCsv());

        AssertSchema(rows, DefectsHeader);
        Assert.AreEqual(2, rows.Count);
        CollectionAssert.AreEqual(new[]
        {
            defect.Id.ToString(), api.Name, api.Id.ToString(), endpoint.Name, endpoint.Id.ToString(),
            endpoint.Method, endpoint.Path, "ExpectedStatus", "High", "Urgent", "Retest", "Tester, QA",
            createdAt.ToString("O"), updatedAt.ToString("O"), retestedAt.ToString("O"), sourceResultId.ToString(),
            defect.Title, "Contains, comma\nsecond line", api.BaseUrl, "Verification", "True"
        }, rows[1].Select(NormalizeNewLines).ToArray());
    }

    [TestMethod]
    public void Exports_WithMissingApiAndEndpoint_PreserveIdsAndEmptyOptionalFields()
    {
        var store = new InMemoryPlatformStore();
        var apiId = Guid.NewGuid();
        AddResult(store, apiId, false, "Missing context");
        AddDefect(store, apiId, DefectSeverity.Low, DefectStatus.New, "Missing context");

        var results = ParseCsv(new ReportsController(store).ExportResultsCsv());
        var defects = ParseCsv(new ReportsController(store).ExportDefectsCsv());

        AssertSchema(results, ResultsHeader);
        AssertSchema(defects, DefectsHeader);
        Assert.AreEqual(apiId.ToString(), results[1][2]);
        Assert.AreEqual(store.GetResults()[0].EndpointId.ToString(), results[1][4]);
        Assert.AreEqual(apiId.ToString(), defects[1][2]);
        Assert.AreEqual(store.GetDefects()[0].EndpointId.ToString(), defects[1][4]);
        foreach (var index in new[] { 1, 3, 5, 6, 9, 10, 13 })
            Assert.AreEqual(string.Empty, results[1][index], $"Result column {ResultsHeader[index]} should be empty.");
        foreach (var index in new[] { 1, 3, 5, 6, 11, 14, 15, 18 })
            Assert.AreEqual(string.Empty, defects[1][index], $"Defect column {DefectsHeader[index]} should be empty.");
    }

    [TestMethod]
    public void Exports_WithoutFilters_IncludeAllRowsNewestFirst()
    {
        var store = new InMemoryPlatformStore();
        var older = new DateTimeOffset(2026, 8, 29, 10, 0, 0, TimeSpan.Zero);
        var newer = older.AddDays(1);
        store.AddResult(new ApiCheckResult { Passed = true, Message = "older", ExecutedAtUtc = older });
        store.AddResult(new ApiCheckResult { Passed = false, Message = "newer", ExecutedAtUtc = newer });
        store.AddDefect(new Defect { Title = "older", Status = DefectStatus.Closed, UpdatedAtUtc = older });
        store.AddDefect(new Defect { Title = "newer", Status = DefectStatus.New, UpdatedAtUtc = newer });

        var results = ParseCsv(new ReportsController(store).ExportResultsCsv());
        var defects = ParseCsv(new ReportsController(store).ExportDefectsCsv());

        AssertSchema(results, ResultsHeader);
        AssertSchema(defects, DefectsHeader);
        Assert.AreEqual(3, results.Count);
        Assert.AreEqual(3, defects.Count);
        CollectionAssert.AreEqual(new[] { "newer", "older" }, results.Skip(1).Select(row => row[12]).ToArray());
        CollectionAssert.AreEqual(new[] { "newer", "older" }, defects.Skip(1).Select(row => row[16]).ToArray());
    }

    [DataTestMethod]
    [DataRow(DefectStatus.New, "Active", true)]
    [DataRow(DefectStatus.Triaged, "Active", true)]
    [DataRow(DefectStatus.Assigned, "Active", true)]
    [DataRow(DefectStatus.InProgress, "Active", true)]
    [DataRow(DefectStatus.Resolved, "Verification", true)]
    [DataRow(DefectStatus.Retest, "Verification", true)]
    [DataRow(DefectStatus.Closed, "Closed", false)]
    [DataRow(DefectStatus.Reopened, "Active", true)]
    [DataRow(DefectStatus.Deferred, "Deferred", false)]
    public void ExportDefectsCsv_MapsEveryLifecycleStatus(DefectStatus status, string stage, bool isOpen)
    {
        var store = new InMemoryPlatformStore();
        AddDefect(store, Guid.NewGuid(), DefectSeverity.Medium, status, "Lifecycle fixture");

        var rows = ParseCsv(new ReportsController(store).ExportDefectsCsv());

        AssertSchema(rows, DefectsHeader);
        Assert.AreEqual(status.ToString(), rows[1][10]);
        Assert.AreEqual(stage, rows[1][19]);
        Assert.AreEqual(isOpen.ToString(), rows[1][20]);
    }

    [DataTestMethod]
    [DataRow("=SUM(1,2)")]
    [DataRow("+SUM(1,2)")]
    [DataRow("-SUM(1,2)")]
    [DataRow("@SUM(1,2)")]
    [DataRow(" \t=SUM(1,2)")]
    public void Exports_NeutralizeFormulaPrefixesInUntrustedFields(string value)
    {
        var store = new InMemoryPlatformStore();
        var api = new RegisteredApi { Name = value, BaseUrl = value };
        store.AddApi(api);
        var endpoint = new ApiEndpoint { ApiId = api.Id, Name = value, Method = value, Path = value };
        store.AddEndpoint(api.Id, endpoint);
        store.AddResult(new ApiCheckResult { ApiId = api.Id, EndpointId = endpoint.Id, CheckType = value, Message = value });
        store.AddDefect(new Defect
        {
            ApiId = api.Id, EndpointId = endpoint.Id, CheckType = value, AssignedTo = value,
            Title = value, Description = value
        });

        var results = ParseCsv(new ReportsController(store).ExportResultsCsv());
        var defects = ParseCsv(new ReportsController(store).ExportDefectsCsv());

        AssertSchema(results, ResultsHeader);
        AssertSchema(defects, DefectsHeader);
        foreach (var index in new[] { 1, 3, 5, 6, 7, 12, 13 })
            Assert.AreEqual("'" + value, results[1][index], $"Result column {ResultsHeader[index]} should neutralize the prefix.");
        foreach (var index in new[] { 1, 3, 5, 6, 7, 11, 16, 17, 18 })
            Assert.AreEqual("'" + value, defects[1][index], $"Defect column {DefectsHeader[index]} should neutralize the prefix.");
    }

    [DataTestMethod]
    [DataRow("apiId")]
    [DataRow("passed")]
    public void ExportResultsCsv_WithMalformedFilter_ReturnsActionableBadRequest(string filter)
    {
        var store = new InMemoryPlatformStore();
        AddResult(store, Guid.NewGuid(), false, "Must not export with an invalid filter");
        var controller = new ReportsController(store);
        // MVC records a conversion error when a query value cannot bind to Guid?/bool?.
        controller.ModelState.AddModelError(filter, "Invalid query value.");

        var result = controller.ExportResultsCsv() as BadRequestObjectResult;

        Assert.IsNotNull(result);
        var message = result.Value as string;
        Assert.IsNotNull(message);
        StringAssert.Contains(message, "apiId as a GUID");
        StringAssert.Contains(message, "passed as true or false");
        StringAssert.Contains(message, "Omit a filter");
    }

    [DataTestMethod]
    [DataRow("apiId")]
    [DataRow("status")]
    [DataRow("severity")]
    public void ExportDefectsCsv_WithMalformedFilter_ReturnsActionableBadRequest(string filter)
    {
        var controller = new ReportsController(new InMemoryPlatformStore());
        controller.ModelState.AddModelError(filter, "Invalid query value.");

        AssertDefectFilterError(controller.ExportDefectsCsv());
    }

    [DataTestMethod]
    [DataRow(-1, 0)]
    [DataRow(999, 0)]
    [DataRow(0, -1)]
    [DataRow(0, 999)]
    public void ExportDefectsCsv_WithUndefinedEnumFilter_ReturnsActionableBadRequest(int status, int severity)
    {
        var controller = new ReportsController(new InMemoryPlatformStore());

        AssertDefectFilterError(controller.ExportDefectsCsv(status: (DefectStatus)status, severity: (DefectSeverity)severity));
    }

    private static void AssertDefectFilterError(IActionResult result)
    {
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        var message = badRequest.Value as string;
        Assert.IsNotNull(message);
        StringAssert.Contains(message, "apiId as a GUID");
        StringAssert.Contains(message, "status as one of New, Triaged");
        StringAssert.Contains(message, "severity as one of Low, Medium, High, Critical");
        StringAssert.Contains(message, "Omit a filter");
    }

    private static List<string[]> ParseCsv(IActionResult result)
    {
        var file = result as FileContentResult;
        Assert.IsNotNull(file);
        using var parser = new TextFieldParser(new StringReader(Encoding.UTF8.GetString(file.FileContents)))
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };
        parser.SetDelimiters(",");
        var rows = new List<string[]>();
        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            Assert.IsNotNull(fields);
            rows.Add(fields);
        }
        return rows;
    }

    private static void AssertSchema(IReadOnlyList<string[]> rows, string[] header)
    {
        Assert.IsTrue(rows.Count > 0, "CSV must include its header even when no rows match.");
        CollectionAssert.AreEqual(header, rows[0]);
        foreach (var row in rows.Skip(1))
            Assert.AreEqual(header.Length, row.Length, "Every logical CSV row must have the same number of columns as the header.");
    }

    private static string NormalizeNewLines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static void AddResult(InMemoryPlatformStore store, Guid apiId, bool passed, string message)
    {
        store.AddResult(new ApiCheckResult
        {
            ApiId = apiId,
            EndpointId = Guid.NewGuid(),
            CheckType = "Availability",
            Passed = passed,
            Message = message
        });
    }

    private static void AddDefect(
        InMemoryPlatformStore store, Guid apiId, DefectSeverity severity,
        DefectStatus status, string title)
    {
        store.AddDefect(new Defect
        {
            ApiId = apiId,
            EndpointId = Guid.NewGuid(),
            Title = title,
            Description = "Fixture",
            Severity = severity,
            Priority = DefectPriority.Medium,
            Status = status
        });
    }
}
