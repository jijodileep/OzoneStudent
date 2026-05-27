using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Audit;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Persistence;

/// <summary>
/// Per-tenant database context. Platform catalog (tenants, settings) uses <see cref="Platform.PlatformDbContext"/>.
/// </summary>
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

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    public DbSet<TenantIsolationProbe> TenantIsolationProbes => Set<TenantIsolationProbe>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    public DbSet<Term> Terms => Set<Term>();

    public DbSet<Grade> Grades => Set<Grade>();

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<Staff> StaffMembers => Set<Staff>();

    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();

    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();

    public DbSet<ProfileDocument> ProfileDocuments => Set<ProfileDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type.Namespace is null
                || !type.Namespace.EndsWith(".Configurations.Platform", StringComparison.Ordinal));

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
