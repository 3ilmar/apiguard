using System.ComponentModel.DataAnnotations;

namespace SecureApiQualityPlatform.Web.Models;

public sealed class RegisteredApi
{
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Url, StringLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public List<ApiEndpoint> Endpoints { get; } = new();
}
