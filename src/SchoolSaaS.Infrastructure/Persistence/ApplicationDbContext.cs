using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContextAccessor tenantContextAccessor)
        : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    public DbSet<TenantIsolationProbe> TenantIsolationProbes => Set<TenantIsolationProbe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyTenantQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType is null
                || !typeof(ITenantEntity).IsAssignableFrom(clrType)
                || clrType.IsDefined(typeof(ExcludeFromTenantFilterAttribute), inherit: true))
            {
                continue;
            }

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetTenantQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(clrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(BuildTenantFilter<TEntity>());
    }

    private Expression<Func<TEntity, bool>> BuildTenantFilter<TEntity>()
        where TEntity : class, ITenantEntity, ISoftDeletable
    {
        return entity =>
            !entity.IsDeleted
            && _tenantContextAccessor.Current.TenantId != null
            && entity.TenantId == _tenantContextAccessor.Current.TenantId;
    }
}
