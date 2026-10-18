using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public interface IRoleProvider
{
    UserRole CurrentRole { get; }
}