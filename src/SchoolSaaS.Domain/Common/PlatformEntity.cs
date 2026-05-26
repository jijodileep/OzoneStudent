namespace SchoolSaaS.Domain.Common;

/// <summary>
/// Base for platform-level entities (e.g. Tenant) that are not scoped by tenant_id.
/// </summary>
[ExcludeFromTenantFilter]
public abstract class PlatformEntity : ISoftDeletable
{
    protected PlatformEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
