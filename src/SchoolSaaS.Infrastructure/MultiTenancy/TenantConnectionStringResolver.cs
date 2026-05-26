using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolSaaS.Infrastructure.Persistence.Platform;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed class TenantConnectionStringResolver(
    PlatformDbContext platformDb,
    ITenantDatabaseCredentialsProvider credentialsProvider,
    IMemoryCache cache) : ITenantConnectionStringResolver
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<string> ResolveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tenant-connection:{tenantId:N}";
        if (cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
        {
            return cached;
        }

        var credentials = await credentialsProvider.GetAsync(tenantId, cancellationToken);
        var connectionString = BuildConnectionString(
            credentials.Server,
            credentials.Port,
            credentials.Database,
            credentials.User,
            credentials.Password);

        cache.Set(cacheKey, connectionString, CacheTtl);
        return connectionString;
    }

    public async Task<string> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenantId = await platformDb.Tenants
            .AsNoTracking()
            .Where(t => t.Slug == slug)
            .Select(t => (Guid?)t.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (tenantId is null)
        {
            throw new InvalidOperationException($"Tenant slug '{slug}' was not found.");
        }

        return await ResolveAsync(tenantId.Value, cancellationToken);
    }

    public string BuildConnectionString(
        string server,
        int port,
        string database,
        string user,
        string password)
    {
        var builder = $"Server={server};Port={port};User={user};Password={password};";
        if (!string.IsNullOrWhiteSpace(database))
        {
            builder += $"Database={database};";
        }

        return builder;
    }
}
