using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Interceptors;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public interface ITenantDbContextFactory
{
    Task<ApplicationDbContext> CreateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed class TenantDbContextFactory(
    ITenantConnectionStringResolver connectionResolver,
    IServiceProvider serviceProvider) : ITenantDbContextFactory
{
    public async Task<ApplicationDbContext> CreateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var connectionString = await connectionResolver.ResolveAsync(tenantId, cancellationToken);
        var requestAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
        var accessor = new TenantContextAccessor(new TenantContext
        {
            TenantId = tenantId,
            UserId = requestAccessor.Current.UserId,
            IsSuperAdmin = requestAccessor.Current.IsSuperAdmin
        });

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, MySqlServerVersionProvider.Version)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                new TenantSaveChangesInterceptor(accessor),
                serviceProvider.GetRequiredService<OutboxSaveChangesInterceptor>());

        return new ApplicationDbContext(optionsBuilder.Options, accessor);
    }
}
