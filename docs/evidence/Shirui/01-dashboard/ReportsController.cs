using System.Text;
using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class ReportsController : Controller
{
    private readonly IPlatformStore _store;

    public ReportsController(IPlatformStore store) => _store = store;

    // Optional query filters are applied before CSV generation. Omitted filters
    // preserve the original export-all behaviour for existing UI links.
    public IActionResult ExportResultsCsv(Guid? apiId = null, bool? passed = null)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid report filter. Use apiId as a GUID and passed as true or false. Omit a filter to include all values.");

        var (apis, endpoints) = BuildLookups();
        var sb = new StringBuilder();
        sb.AppendLine("ExecutedAtUtc,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Passed,ActualStatusCode,ResponseTimeMs,FailureSeverity,Message,ApiBaseUrl,Outcome");

        foreach (var r in _store.GetResults()
            .Where(r => (!apiId.HasValue || r.ApiId == apiId.Value) &&
                        (!passed.HasValue || r.Passed == passed.Value))
            .OrderByDescending(r => r.ExecutedAtUtc))
        {
            apis.TryGetValue(r.ApiId, out var api);
            endpoints.TryGetValue(r.EndpointId, out var endpoint);

            sb.AppendLine(string.Join(',',
                Csv(r.ExecutedAtUtc.ToString("O")),
                CsvText(api?.Name),
                Csv(r.ApiId.ToString()),
                CsvText(endpoint?.Name),
                Csv(r.EndpointId.ToString()),
                CsvText(endpoint?.Method),
                CsvText(endpoint?.Path),
                CsvText(r.CheckType),
                Csv(r.Passed.ToString()),
                Csv(r.ActualStatusCode?.ToString()),
                Csv(r.ResponseTimeMs?.ToString()),
                Csv(r.FailureSeverity.ToString()),
                CsvText(r.Message),
                CsvText(api?.BaseUrl),
                Csv(r.Passed ? "Pass" : "Fail")));
        }

        return CsvFile(sb, "api-check-results");
    }

    public IActionResult ExportDefectsCsv(
        DefectStatus? status = null,
        DefectSeverity? severity = null,
        Guid? apiId = null)
    {
        if (!ModelState.IsValid ||
            (status.HasValue && !Enum.IsDefined(status.Value)) ||
            (severity.HasValue && !Enum.IsDefined(severity.Value)))
        {
            return BadRequest($"Invalid report filter. Use apiId as a GUID, status as one of {string.Join(", ", Enum.GetNames<DefectStatus>())}, and severity as one of {string.Join(", ", Enum.GetNames<DefectSeverity>())}. Omit a filter to include all values.");
        }

        var (apis, endpoints) = BuildLookups();
        var sb = new StringBuilder();
        sb.AppendLine("DefectId,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Severity,Priority,Status,AssignedTo,CreatedAtUtc,UpdatedAtUtc,RetestedAtUtc,SourceCheckResultId,Title,Description,ApiBaseUrl,LifecycleStage,IsOpen");

        foreach (var d in _store.GetDefects()
            .Where(d => (!status.HasValue || d.Status == status.Value) &&
                        (!severity.HasValue || d.Severity == severity.Value) &&
                        (!apiId.HasValue || d.ApiId == apiId.Value))
            .OrderByDescending(d => d.UpdatedAtUtc))
        {
            apis.TryGetValue(d.ApiId, out var api);
            endpoints.TryGetValue(d.EndpointId, out var endpoint);

            sb.AppendLine(string.Join(',',
                Csv(d.Id.ToString()),
                CsvText(api?.Name),
                Csv(d.ApiId.ToString()),
                CsvText(endpoint?.Name),
                Csv(d.EndpointId.ToString()),
                CsvText(endpoint?.Method),
                CsvText(endpoint?.Path),
                CsvText(d.CheckType),
                Csv(d.Severity.ToString()),
                Csv(d.Priority.ToString()),
                Csv(d.Status.ToString()),
                CsvText(d.AssignedTo),
                Csv(d.CreatedAtUtc.ToString("O")),
                Csv(d.UpdatedAtUtc.ToString("O")),
                Csv(d.RetestedAtUtc?.ToString("O")),
                Csv(d.SourceCheckResultId?.ToString()),
                CsvText(d.Title),
                CsvText(d.Description),
                CsvText(api?.BaseUrl),
                Csv(LifecycleStage(d.Status)),
                Csv(IsOpen(d.Status).ToString())));
        }

        return CsvFile(sb, "defects");
    }

    private (IReadOnlyDictionary<Guid, RegisteredApi> Apis, IReadOnlyDictionary<Guid, ApiEndpoint> Endpoints) BuildLookups()
    {
        var apis = _store.GetApis();
        return (
            apis.ToDictionary(a => a.Id),
            apis.SelectMany(a => a.Endpoints).ToDictionary(e => e.Id));
    }

    private FileContentResult CsvFile(StringBuilder sb, string prefix)
    {
        var fileName = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

    private static bool IsOpen(DefectStatus status) =>
        status is not (DefectStatus.Closed or DefectStatus.Deferred);

    private static string LifecycleStage(DefectStatus status) => status switch
    {
        DefectStatus.Closed => "Closed",
        DefectStatus.Deferred => "Deferred",
        DefectStatus.Resolved or DefectStatus.Retest => "Verification",
        _ => "Active"
    };

    // Quoting protects CSV structure; a leading apostrophe additionally
    // prevents spreadsheet programs interpreting untrusted text as a formula.
    private static string CsvText(string? value)
    {
        var safeValue = value ?? string.Empty;
        var trimmed = safeValue.TrimStart();
        if (trimmed.Length > 0 && "=+-@".Contains(trimmed[0]))
            safeValue = "'" + safeValue;
        return Csv(safeValue);
    }

    private static string Csv(string? value)
    {
        var safeValue = value ?? string.Empty;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}
