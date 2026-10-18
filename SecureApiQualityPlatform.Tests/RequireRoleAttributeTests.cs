using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Controllers;
using SecureApiQualityPlatform.Web.Filters;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class RequireRoleAttributeTests
{
    private sealed class FakeRoleProvider : IRoleProvider
    {
        public FakeRoleProvider(UserRole role) => CurrentRole = role;
        public UserRole CurrentRole { get; }
    }

    private sealed class DummyController : Controller { }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) =>
            new Dictionary<string, object?>();
        public void SaveTempData(HttpContext context, IDictionary<string, object?> values) { }
    }

    private static ActionExecutingContext Ctx(UserRole role)
    {
        var services = new ServiceCollection()
            .AddSingleton<IRoleProvider>(new FakeRoleProvider(role))
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        var route = new RouteData();
        route.Values["controller"] = "Defects";
        var controller = new DummyController
        {
            TempData = new TempDataDictionary(http, new FakeTempDataProvider())
        };
        return new ActionExecutingContext(
            new ActionContext(http, route, new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller);
    }

    [TestMethod]
    public void Viewer_IsDeniedOnTesterAction_AndActionDoesNotRun()
    {
        var ctx = Ctx(UserRole.Viewer);
        new RequireRoleAttribute(UserRole.Tester).OnActionExecuting(ctx);
        Assert.IsInstanceOfType(ctx.Result, typeof(RedirectToActionResult));
        StringAssert.Contains(((Controller)ctx.Controller).TempData["Error"]!.ToString()!, "Tester");
    }

    [TestMethod]
    public void Tester_IsAllowedOnTesterAction()
    {
        var ctx = Ctx(UserRole.Tester);
        new RequireRoleAttribute(UserRole.Tester).OnActionExecuting(ctx);
        Assert.IsNull(ctx.Result);
    }

    [TestMethod]
    public void Tester_IsDeniedOnAdminAction()
    {
        var ctx = Ctx(UserRole.Tester);
        new RequireRoleAttribute(UserRole.Admin).OnActionExecuting(ctx);
        Assert.IsInstanceOfType(ctx.Result, typeof(RedirectToActionResult));
    }

    [TestMethod]
    public void Admin_IsAllowedOnAdminAction()
    {
        var ctx = Ctx(UserRole.Admin);
        new RequireRoleAttribute(UserRole.Admin).OnActionExecuting(ctx);
        Assert.IsNull(ctx.Result);
    }

    // Proves the filter is actually attached to the real endpoints, so removing an attribute fails a test.
    [TestMethod]
    [DataRow(typeof(DefectsController), "Update", UserRole.Tester)]
    [DataRow(typeof(ApisController), "RunChecks", UserRole.Tester)]
    [DataRow(typeof(ApisController), "AddEndpoint", UserRole.Admin)]
    [DataRow(typeof(ApisController), "Create", UserRole.Admin)]
    public void ProtectedAction_DeclaresMinimumRole(Type controller, string action, UserRole expected)
    {
        var attributes = controller.GetMethods()
            .Where(m => m.Name == action)
            .SelectMany(m => m.GetCustomAttributes(typeof(RequireRoleAttribute), false))
            .Cast<RequireRoleAttribute>()
            .ToList();

        Assert.IsTrue(attributes.Count > 0, $"{controller.Name}.{action} has no RequireRole attribute.");
        Assert.IsTrue(attributes.All(a => a.Minimum == expected));
    }
}