using System;
using Microsoft.AspNetCore.Mvc;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Controllers;

public sealed class RoleController : Controller
{
    private const string CookieName = "apiguard_role";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetRole(string role, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(role) || !Enum.TryParse<UserRole>(role, true, out var parsed) || !Enum.IsDefined(typeof(UserRole), parsed))
        {
            TempData["Error"] = "Invalid role selected.";
            return SafeRedirect(returnUrl);
        }

        Response.Cookies.Append(CookieName, parsed.ToString(), new CookieOptions
        {
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = false,
            IsEssential = true
        });

        return SafeRedirect(returnUrl);
    }

    private IActionResult SafeRedirect(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
