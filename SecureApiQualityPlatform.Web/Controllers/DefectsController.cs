using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class DefectsController : Controller
{
    private readonly IPlatformStore _store;

    public DefectsController(IPlatformStore store) => _store = store;

    public IActionResult Index() => View(_store.GetDefects());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(Guid id, DefectStatus status, DefectPriority priority, string? assignedTo)
    {
        var defect = _store.GetDefect(id);
        if (defect is null) return NotFound();
        defect.Status = status;
        defect.Priority = priority;
        defect.AssignedTo = string.IsNullOrWhiteSpace(assignedTo) ? null : assignedTo.Trim();
        _store.UpdateDefect(defect);
        return RedirectToAction(nameof(Index));
    }
}
