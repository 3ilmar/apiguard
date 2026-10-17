using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class CookieRoleProviderTests
{
    [TestMethod]
    public void NoCookie_ReturnsAdmin()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var provider = new CookieRoleProvider(accessor);
        Assert.AreEqual(UserRole.Admin, provider.CurrentRole);
    }

    [TestMethod]
    public void ViewerCookie_ReturnsViewer()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Request.Headers["Cookie"] = "apiguard_role=Viewer";
        var provider = new CookieRoleProvider(accessor);
        Assert.AreEqual(UserRole.Viewer, provider.CurrentRole);
    }

    [TestMethod]
    public void GarbageCookie_ReturnsAdmin()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Request.Headers["Cookie"] = "apiguard_role=notarole";
        var provider = new CookieRoleProvider(accessor);
        Assert.AreEqual(UserRole.Admin, provider.CurrentRole);
    }

    [TestMethod]
    public void Numeric7_ReturnsAdmin()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext.Request.Headers["Cookie"] = "apiguard_role=7";
        var provider = new CookieRoleProvider(accessor);
        Assert.AreEqual(UserRole.Admin, provider.CurrentRole);
    }
}
