using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Persistence;

internal sealed class DesignTimeTenantContextAccessor : ITenantContextAccessor
{
    public ITenantContext Current { get; } = new TenantContext();
}
