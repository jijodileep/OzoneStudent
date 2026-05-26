namespace SchoolSaaS.Infrastructure.MultiTenancy;

public interface ITenantConnectionStringResolver
{
    Task<string> ResolveAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<string> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default);

    string BuildConnectionString(
        string server,
        int port,
        string database,
        string user,
        string password);
}
