using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class RoleProviderTests
{
    private static CookieRoleProvider Make(string? cookieValue)
    {
        var context = new DefaultHttpContext();
        if (cookieValue is not null)
        {
            context.Request.Headers["Cookie"] = $"{CookieRoleProvider.CookieName}={cookieValue}";
        }
        return new CookieRoleProvider(new HttpContextAccessor { HttpContext = context });
    }

    [TestMethod]
    public void NoCookie_DefaultsToViewer() =>
        Assert.AreEqual(UserRole.Viewer, Make(null).CurrentRole);

    [TestMethod]
    public void AdminCookie_ReturnsAdmin() =>
        Assert.AreEqual(UserRole.Admin, Make("Admin").CurrentRole);

    [TestMethod]
    public void GarbageCookie_DefaultsToViewer() =>
        Assert.AreEqual(UserRole.Viewer, Make("hacker").CurrentRole);

    [TestMethod]
    public void UndefinedNumericCookie_DefaultsToViewer() =>
        Assert.AreEqual(UserRole.Viewer, Make("7").CurrentRole);
}