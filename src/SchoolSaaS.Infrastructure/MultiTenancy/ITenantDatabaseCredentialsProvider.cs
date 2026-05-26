using SchoolSaaS.Domain.Platform;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public interface ITenantDatabaseCredentialsProvider
{
    Task<TenantDatabaseCredentials> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);

    TenantDatabaseCredentials GetFromTenant(Tenant tenant);
}
