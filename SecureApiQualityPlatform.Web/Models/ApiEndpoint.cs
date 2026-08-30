using System.ComponentModel.DataAnnotations;

namespace SecureApiQualityPlatform.Web.Models;

public sealed class ApiEndpoint
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ApiId { get; init; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Path { get; set; } = "/";

    [Required, StringLength(10)]
    public string Method { get; set; } = "GET";

    [Range(100, 599)]
    public int ExpectedStatusCode { get; set; } = 200;

    [Range(50, 60000)]
    public int MaxResponseTimeMs { get; set; } = 1500;

    public bool RequiresAuthentication { get; set; }

    [StringLength(100)]
    public string? ExpectedContentType { get; set; }
}
