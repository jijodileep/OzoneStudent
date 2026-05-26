using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed class TenantDatabaseProvisioner(
    IConfiguration configuration,
    PlatformDbContext platformDb,
    ITenantConnectionStringResolver connectionResolver,
    ITenantDatabaseCredentialsProvider credentialsProvider,
    IOptions<TenancyOptions> tenancyOptions) : ITenantDatabaseProvisioner
{
    public async Task ProvisionAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenant.DbName))
        {
            throw new InvalidOperationException("Tenant.DbName must be set before provisioning.");
        }

        await CreateDatabaseIfNotExistsAsync(tenant.Id, cancellationToken);

        var connectionString = await connectionResolver.ResolveAsync(tenant.Id, cancellationToken);

        await using var tenantDb = await CreateMigratorContextAsync(connectionString, cancellationToken);
        await tenantDb.Database.MigrateAsync(cancellationToken);
    }

    public async Task SeedTenantPermissionsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var catalog = await platformDb.Permissions.AsNoTracking().ToListAsync(cancellationToken);
        var connectionString = await connectionResolver.ResolveAsync(tenantId, cancellationToken);

        await using var tenantDb = await CreateMigratorContextAsync(connectionString, cancellationToken);

        var existingCodes = await tenantDb.Permissions
            .IgnoreQueryFilters()
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var existingSet = existingCodes.ToHashSet(StringComparer.Ordinal);

        foreach (var permission in catalog)
        {
            if (existingSet.Contains(permission.Code))
            {
                continue;
            }

            tenantDb.Permissions.Add(new Permission
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Module = permission.Module,
                Description = permission.Description,
                IsSystem = permission.IsSystem
            });
        }

        await tenantDb.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateDatabaseIfNotExistsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var credentials = await credentialsProvider.GetAsync(tenantId, cancellationToken);
        var adminConnection = ResolveProvisioningConnection(credentials);

        await using var connection = new MySqlConnection(adminConnection);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"CREATE DATABASE IF NOT EXISTS `{credentials.Database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Connects to the tenant's MySQL server (may differ from the platform host) to create the database.
    /// </summary>
    private string ResolveProvisioningConnection(TenantDatabaseCredentials tenantCredentials)
    {
        var opts = tenancyOptions.Value;
        var provisioningTemplate = configuration.GetConnectionString("Provisioning");

        if (!string.IsNullOrWhiteSpace(provisioningTemplate))
        {
            var builder = new MySqlConnectionStringBuilder(provisioningTemplate)
            {
                Server = tenantCredentials.Server,
                Port = (uint)tenantCredentials.Port,
                Database = string.Empty
            };

            return builder.ConnectionString;
        }

        var user = string.IsNullOrWhiteSpace(opts.ProvisioningDbUser)
            ? tenantCredentials.User
            : opts.ProvisioningDbUser;
        var password = string.IsNullOrWhiteSpace(opts.ProvisioningDbPassword)
            ? tenantCredentials.Password
            : opts.ProvisioningDbPassword;

        return connectionResolver.BuildConnectionString(
            tenantCredentials.Server,
            tenantCredentials.Port,
            string.Empty,
            user,
            password);
    }

    private static Task<ApplicationDbContext> CreateMigratorContextAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        var accessor = new TenantContextAccessor(new TenantContext());
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, MySqlServerVersionProvider.Version)
            .UseSnakeCaseNamingConvention()
            .Options;

        return Task.FromResult(new ApplicationDbContext(options, accessor));
    }
}
