namespace SchoolSaaS.Shared.MultiTenancy;

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    public TenantContextAccessor(TenantContext tenantContext) => Current = tenantContext;

    public ITenantContext Current { get; }
}
