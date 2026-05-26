namespace SchoolSaaS.Shared.MultiTenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }

    Guid? BranchId { get; }

    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsSuperAdmin { get; }
}
