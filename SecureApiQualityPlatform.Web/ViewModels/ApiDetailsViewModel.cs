using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.ViewModels;

public sealed class ApiDetailsViewModel
{
    public RegisteredApi Api { get; init; } = new();
    public ApiEndpoint NewEndpoint { get; init; } = new();
    public IReadOnlyList<ApiCheckResult> RecentResults { get; init; } = Array.Empty<ApiCheckResult>();
}
