using System.Text;
using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class ReportsController : Controller
{
    private readonly IPlatformStore _store;

    public ReportsController(IPlatformStore store) => _store = store;

    public IActionResult ExportResultsCsv()
    {
        var (apis, endpoints) = BuildLookups();
        var sb = new StringBuilder();
        sb.AppendLine("ExecutedAtUtc,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Passed,ActualStatusCode,ResponseTimeMs,FailureSeverity,Message");

        foreach (var r in _store.GetResults().OrderByDescending(r => r.ExecutedAtUtc))
        {
            apis.TryGetValue(r.ApiId, out var api);
            endpoints.TryGetValue(r.EndpointId, out var endpoint);

            sb.AppendLine(string.Join(',',
                Csv(r.ExecutedAtUtc.ToString("O")),
                Csv(api?.Name),
                Csv(r.ApiId.ToString()),
                Csv(endpoint?.Name),
                Csv(r.EndpointId.ToString()),
                Csv(endpoint?.Method),
                Csv(endpoint?.Path),
                Csv(r.CheckType),
                Csv(r.Passed.ToString()),
                Csv(r.ActualStatusCode?.ToString()),
                Csv(r.ResponseTimeMs?.ToString()),
                Csv(r.FailureSeverity.ToString()),
                Csv(r.Message)));
        }

        return CsvFile(sb, "api-check-results");
    }

    public IActionResult ExportDefectsCsv()
    {
        var (apis, endpoints) = BuildLookups();
        var sb = new StringBuilder();
        sb.AppendLine("DefectId,ApiName,ApiId,EndpointName,EndpointId,Method,Path,CheckType,Severity,Priority,Status,AssignedTo,CreatedAtUtc,UpdatedAtUtc,RetestedAtUtc,SourceCheckResultId,Title,Description");

        foreach (var d in _store.GetDefects().OrderByDescending(d => d.UpdatedAtUtc))
        {
            apis.TryGetValue(d.ApiId, out var api);
            endpoints.TryGetValue(d.EndpointId, out var endpoint);

            sb.AppendLine(string.Join(',',
                Csv(d.Id.ToString()),
                Csv(api?.Name),
                Csv(d.ApiId.ToString()),
                Csv(endpoint?.Name),
                Csv(d.EndpointId.ToString()),
                Csv(endpoint?.Method),
                Csv(endpoint?.Path),
                Csv(d.CheckType),
                Csv(d.Severity.ToString()),
                Csv(d.Priority.ToString()),
                Csv(d.Status.ToString()),
                Csv(d.AssignedTo),
                Csv(d.CreatedAtUtc.ToString("O")),
                Csv(d.UpdatedAtUtc.ToString("O")),
                Csv(d.RetestedAtUtc?.ToString("O")),
                Csv(d.SourceCheckResultId?.ToString()),
                Csv(d.Title),
                Csv(d.Description)));
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

    private static string Csv(string? value)
    {
        var safeValue = value ?? string.Empty;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}
