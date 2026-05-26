using SchoolSaaS.Domain.Platform;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public interface ITenantDatabaseProvisioner
{
    /// <summary>Creates the MySQL database (if missing) and applies tenant schema migrations.</summary>
    Task ProvisionAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>Copies global permission catalog into the tenant database.</summary>
    Task SeedTenantPermissionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
