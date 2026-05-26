namespace SchoolSaaS.Shared.MultiTenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? UserId { get; set; }

    public bool IsAuthenticated { get; set; }

    public bool IsSuperAdmin { get; set; }
}
