using System.Text;
using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class ReportsController : Controller
{
    private readonly IPlatformStore _store;

    public ReportsController(IPlatformStore store) => _store = store;

    public IActionResult ExportResultsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ExecutedAtUtc,ApiId,EndpointId,CheckType,Passed,StatusCode,ResponseTimeMs,Message");
        foreach (var r in _store.GetResults())
        {
            sb.AppendLine(string.Join(',',
                Csv(r.ExecutedAtUtc.ToString("O")), Csv(r.ApiId.ToString()), Csv(r.EndpointId.ToString()),
                Csv(r.CheckType), Csv(r.Passed.ToString()), Csv(r.ActualStatusCode?.ToString() ?? ""),
                Csv(r.ResponseTimeMs?.ToString() ?? ""), Csv(r.Message)));
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "api-check-results.csv");
    }

    public IActionResult ExportDefectsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("DefectId,CheckType,Severity,Priority,Status,AssignedTo,UpdatedAtUtc,Title,Description");
        foreach (var d in _store.GetDefects())
        {
            sb.AppendLine(string.Join(',', Csv(d.Id.ToString()), Csv(d.CheckType), Csv(d.Severity.ToString()),
                Csv(d.Priority.ToString()), Csv(d.Status.ToString()), Csv(d.AssignedTo ?? ""),
                Csv(d.UpdatedAtUtc.ToString("O")), Csv(d.Title), Csv(d.Description)));
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "defects.csv");
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
