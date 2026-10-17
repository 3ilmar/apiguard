using Microsoft.Extensions.FileProviders;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class CookieRoleProvider : IRoleProvider
{
    public const string CookieName = "apiguard_role";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieRoleProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public UserRole CurrentRole
    {
        get
        {
            var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
            if (cookies is null || !cookies.TryGetValue(CookieName, out var value))
                return UserRole.Admin;

            // Only accept a real enum name or a defined number; anything else falls back to the default.
            if (Enum.TryParse<UserRole>(value, ignoreCase: true, out var role) && Enum.IsDefined(typeof(UserRole), role))
                return role;

            return UserRole.Admin;
        }
    }
}