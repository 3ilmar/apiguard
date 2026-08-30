using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly DashboardService _dashboard;

    public HomeController(DashboardService dashboard) => _dashboard = dashboard;

    public IActionResult Index() => View(_dashboard.Build());

    [Route("Home/Error")]
    public IActionResult Error() => View();
}
