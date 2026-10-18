using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Web.Filters;

/// <summary>
/// Server-side check that runs before the action. The role comes from IRoleProvider, which in this
/// prototype is a user-selected demo cookie, so this demonstrates RBAC behaviour but is not authentication.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireRoleAttribute : ActionFilterAttribute
{
    public UserRole Minimum { get; }

    public RequireRoleAttribute(UserRole minimum) => Minimum = minimum;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var provider = context.HttpContext.RequestServices.GetRequiredService<IRoleProvider>();
        if (provider.CurrentRole >= Minimum) return;

        if (context.Controller is Controller controller)
        {
            controller.TempData["Error"] =
                $"Your role ({provider.CurrentRole}) cannot do this. {Minimum} or higher is required.";
        }

        context.Result = new RedirectToActionResult(
            "Index", context.RouteData.Values["controller"]?.ToString(), null);
    }
}