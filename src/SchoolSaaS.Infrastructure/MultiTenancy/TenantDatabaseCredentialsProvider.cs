using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.MultiTenancy;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Infrastructure.Persistence.Platform;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed class TenantDatabaseCredentialsProvider(
    PlatformDbContext platformDb,
    ITenantDbCredentialProtector credentialProtector,
    IOptions<TenancyOptions> tenancyOptions) : ITenantDatabaseCredentialsProvider
{
    public async Task<TenantDatabaseCredentials> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await platformDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant {tenantId} was not found in the platform catalog.");

        return GetFromTenant(tenant);
    }

    public TenantDatabaseCredentials GetFromTenant(Tenant tenant)
    {
        var opts = tenancyOptions.Value;
        var server = string.IsNullOrWhiteSpace(tenant.DbServer) ? opts.DefaultDbServer : tenant.DbServer;
        var port = tenant.DbPort > 0 ? tenant.DbPort : opts.DefaultDbPort;
        var database = tenant.DbName;
        var user = string.IsNullOrWhiteSpace(tenant.DbUser) ? opts.DefaultDbUser : tenant.DbUser;
        var storedPassword = string.IsNullOrWhiteSpace(tenant.DbPassword)
            ? opts.DefaultDbPassword
            : credentialProtector.Unprotect(tenant.DbPassword);

        return new TenantDatabaseCredentials(server, port, database, user, storedPassword);
    }
}
