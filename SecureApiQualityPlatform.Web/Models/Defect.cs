using System.ComponentModel.DataAnnotations;

namespace SecureApiQualityPlatform.Web.Models;

public sealed class Defect
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ApiId { get; init; }
    public Guid EndpointId { get; init; }
    public Guid? SourceCheckResultId { get; set; }

    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string CheckType { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public DefectPriority Priority { get; set; }
    public DefectStatus Status { get; set; } = DefectStatus.New;

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RetestedAtUtc { get; set; }
}
