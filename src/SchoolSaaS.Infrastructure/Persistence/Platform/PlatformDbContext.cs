using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Infrastructure.Persistence.Configurations;
using SchoolSaaS.Infrastructure.Persistence.Configurations.Platform;

namespace SchoolSaaS.Infrastructure.Persistence.Platform;

/// <summary>
/// Platform catalog database: tenant registry and global permission catalog only.
/// </summary>
public sealed class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();

    public DbSet<Permission> Permissions => Set<Permission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSettingConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
