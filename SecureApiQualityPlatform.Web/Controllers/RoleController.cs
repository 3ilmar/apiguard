using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class RoleController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetRole(string role, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(role) ||
            !Enum.TryParse<UserRole>(role, true, out var parsed) ||
            !Enum.IsDefined(parsed))
        {
            TempData["Error"] = "Invalid role selected.";
            return SafeRedirect(returnUrl);
        }

        Response.Cookies.Append(CookieRoleProvider.CookieName, parsed.ToString(), new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            IsEssential = true
        });

        return SafeRedirect(returnUrl);
    }

    private IActionResult SafeRedirect(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
}