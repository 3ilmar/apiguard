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
    public void NoCookie_DefaultsToAdmin() =>
        Assert.AreEqual(UserRole.Admin, Make(null).CurrentRole);

    [TestMethod]
    public void ViewerCookie_ReturnsViewer() =>
        Assert.AreEqual(UserRole.Viewer, Make("Viewer").CurrentRole);

    [TestMethod]
    public void GarbageCookie_DefaultsToAdmin() =>
        Assert.AreEqual(UserRole.Admin, Make("hacker").CurrentRole);

    [TestMethod]
    public void UndefinedNumericCookie_DefaultsToAdmin() =>
        Assert.AreEqual(UserRole.Admin, Make("7").CurrentRole);
}