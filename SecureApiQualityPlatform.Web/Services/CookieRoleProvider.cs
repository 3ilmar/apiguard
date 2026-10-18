using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

/// <summary>
/// Reads the demo role from a cookie. This is NOT authentication: the cookie is chosen by the user,
/// so it only demonstrates role-based behaviour. Missing or invalid values fall back to the
/// least-privileged role (fail closed).
/// </summary>
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
                return UserRole.Viewer;

            return Enum.TryParse<UserRole>(value, ignoreCase: true, out var role) && Enum.IsDefined(role)
                ? role
                : UserRole.Viewer;
        }
    }
}