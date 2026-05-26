using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Audit;

/// <summary>
/// Append-only audit trail. Not soft-deleted; excluded from tenant auto-filter.
/// </summary>
[ExcludeFromTenantFilter]
public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public string? Description { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public string? MetadataJson { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? CorrelationId { get; set; }

    public string Source { get; set; } = "Api";

    public string Outcome { get; set; } = "Success";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
