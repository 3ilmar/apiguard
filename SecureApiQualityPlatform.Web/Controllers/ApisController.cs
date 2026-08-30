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
        if (!ModelState.IsValid) return View(model);
        var safety = await _urlSafety.ValidateAsync(model.BaseUrl);
        if (!safety.IsSafe)
        {
            ModelState.AddModelError(nameof(model.BaseUrl), safety.Reason);
            return View(model);
        }
        _store.AddApi(model);
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
        if (api is null) return NotFound();

        endpoint = new ApiEndpoint
        {
            ApiId = id,
            Name = endpoint.Name,
            Path = endpoint.Path,
            Method = endpoint.Method,
            ExpectedStatusCode = endpoint.ExpectedStatusCode,
            MaxResponseTimeMs = endpoint.MaxResponseTimeMs,
            RequiresAuthentication = endpoint.RequiresAuthentication,
            ExpectedContentType = endpoint.ExpectedContentType
        };

        if (!TryValidateModel(endpoint))
        {
            return View("Details", new ApiDetailsViewModel
            {
                Api = api,
                NewEndpoint = endpoint,
                RecentResults = _store.GetResultsForApi(api.Id).Take(25).ToArray()
            });
        }

        _store.AddEndpoint(id, endpoint);
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
