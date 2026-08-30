using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;
using SecureApiQualityPlatform.Web.ViewModels;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class ApisController : Controller
{
    private readonly IPlatformStore _store;
    private readonly UrlSafetyService _urlSafety;
    private readonly ApiCheckService _checks;

    public ApisController(IPlatformStore store, UrlSafetyService urlSafety, ApiCheckService checks)
    {
        _store = store;
        _urlSafety = urlSafety;
        _checks = checks;
    }

    public IActionResult Index() => View(_store.GetApis());

    [HttpGet]
    public IActionResult Create() => View(new RegisteredApi());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisteredApi model)
    {
        model.Name = model.Name?.Trim() ?? string.Empty;
        model.BaseUrl = model.BaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
        model.Description = model.Description?.Trim() ?? string.Empty;

        ModelState.Clear();

        if (!TryValidateModel(model))
        {
            return View(model);
        }

        var safety = await _urlSafety.ValidateAsync(model.BaseUrl);

        if (!safety.IsSafe)
        {
            ModelState.AddModelError(nameof(model.BaseUrl), safety.Reason);
            return View(model);
        }

        if (!_store.TryAddApi(model))
        {
            ModelState.AddModelError(
                nameof(model.BaseUrl),
                "An API with this base URL is already registered.");

            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public IActionResult Details(Guid id)
    {
        var api = _store.GetApi(id);
        if (api is null) return NotFound();
        return View(new ApiDetailsViewModel
        {
            Api = api,
            NewEndpoint = new ApiEndpoint { ApiId = api.Id },
            RecentResults = _store.GetResultsForApi(api.Id).Take(25).ToArray()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddEndpoint(Guid id, ApiEndpoint endpoint)
    {
        var api = _store.GetApi(id);

        if (api is null)
        {
            return NotFound();
        }

        var normalizedPath = endpoint.Path?.Trim() ?? string.Empty;

        if (normalizedPath.Contains("://", StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith("//") ||
            normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Any(segment => segment == ".."))
        {
            ModelState.AddModelError(
                nameof(endpoint.Path),
                "Endpoint path must stay within the registered API.");

            return View("Details", new ApiDetailsViewModel
            {
                Api = api,
                NewEndpoint = endpoint,
                RecentResults = _store.GetResultsForApi(api.Id).Take(25).ToArray()
            });
        }

        if (!string.IsNullOrWhiteSpace(normalizedPath))
        {
            if (!normalizedPath.StartsWith('/'))
            {
                normalizedPath = "/" + normalizedPath;
            }

            if (normalizedPath.Length > 1)
            {
                normalizedPath = normalizedPath.TrimEnd('/');
            }
        }

        endpoint = new ApiEndpoint
        {
            ApiId = id,
            Name = endpoint.Name?.Trim() ?? string.Empty,
            Path = normalizedPath,
            Method = endpoint.Method?.Trim().ToUpperInvariant() ?? string.Empty,
            ExpectedStatusCode = endpoint.ExpectedStatusCode,
            MaxResponseTimeMs = endpoint.MaxResponseTimeMs,
            RequiresAuthentication = endpoint.RequiresAuthentication,
            ExpectedContentType = string.IsNullOrWhiteSpace(endpoint.ExpectedContentType)
                ? null
                : endpoint.ExpectedContentType.Trim()
        };

        ModelState.Clear();

        if (!TryValidateModel(endpoint))
        {
            return View("Details", new ApiDetailsViewModel
            {
                Api = api,
                NewEndpoint = endpoint,
                RecentResults = _store.GetResultsForApi(api.Id).Take(25).ToArray()
            });
        }

        if (!_store.TryAddEndpoint(id, endpoint))
        {
            ModelState.AddModelError(
                string.Empty,
                "This method and path are already registered for this API.");

            return View("Details", new ApiDetailsViewModel
            {
                Api = api,
                NewEndpoint = endpoint,
                RecentResults = _store.GetResultsForApi(api.Id).Take(25).ToArray()
            });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunChecks(Guid id)
    {
        await _checks.RunApiAsync(id, HttpContext.RequestAborted);
        return RedirectToAction(nameof(Details), new { id });
    }
}
