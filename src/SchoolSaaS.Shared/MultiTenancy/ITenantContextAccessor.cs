namespace SchoolSaaS.Shared.MultiTenancy;

public interface ITenantContextAccessor
{
    ITenantContext Current { get; }
}
