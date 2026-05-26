using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Application.Abstractions.MultiTenancy;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Infrastructure.MultiTenancy;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.MultiTenancy;
using DefaultSystemRoles = SchoolSaaS.Shared.Authorization.DefaultSystemRoles;

namespace SchoolSaaS.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public const string DefaultTenantSlug = "demo";
    public const string DefaultAdminEmail = "admin@demo.school";
    public const string DefaultAdminPassword = "Admin123!ChangeMe";
    public const string TenantAdminRoleName = DefaultSystemRoles.TenantAdmin;

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var provisioner = scope.ServiceProvider.GetRequiredService<ITenantDatabaseProvisioner>();
        var tenantDbFactory = scope.ServiceProvider.GetRequiredService<ITenantDbContextFactory>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var tenancyOptions = scope.ServiceProvider.GetRequiredService<IOptions<TenancyOptions>>().Value;
        var credentialProtector = scope.ServiceProvider.GetRequiredService<ITenantDbCredentialProtector>();
        var permissionResolver = scope.ServiceProvider.GetRequiredService<IPermissionResolver>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");

        await platformDb.Database.MigrateAsync(cancellationToken);
        await SeedPlatformPermissionsAsync(platformDb, cancellationToken);

        var tenant = await platformDb.Tenants
            .FirstOrDefaultAsync(t => t.Slug == DefaultTenantSlug, cancellationToken);

        if (tenant is not null)
        {
            await EnsureTenantPasswordEncryptedAsync(platformDb, tenant, credentialProtector, cancellationToken);
        }

        if (tenant is null)
        {
            var dbName = $"{tenancyOptions.DatabaseNamePrefix}{DefaultTenantSlug}";
            tenant = new Tenant
            {
                Name = "Demo School",
                Slug = DefaultTenantSlug,
                DbServer = tenancyOptions.DefaultDbServer,
                DbPort = tenancyOptions.DefaultDbPort,
                DbName = dbName,
                DbUser = tenancyOptions.DefaultDbUser,
                DbPassword = credentialProtector.Protect(tenancyOptions.DefaultDbPassword),
                Plan = "free",
                Status = TenantStatus.Active
            };
            platformDb.Tenants.Add(tenant);
            await platformDb.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Registered demo tenant {Slug} with database {Database}",
                DefaultTenantSlug,
                dbName);
        }

        await provisioner.ProvisionAsync(tenant, cancellationToken);
        await provisioner.SeedTenantPermissionsAsync(tenant.Id, cancellationToken);

        await using (var tenantDb = await tenantDbFactory.CreateAsync(tenant.Id, cancellationToken))
        {
            await DefaultSystemRolesSeeder.SeedAsync(tenantDb, tenant.Id, cancellationToken);
        }
        await SyncTenantAdminRolePermissionsAsync(
            tenantDbFactory,
            permissionResolver,
            tenant.Id,
            logger,
            cancellationToken);

        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        tenantContext.TenantId = tenant.Id;

        await SeedTenantAdminAsync(
            tenantDbFactory,
            passwordHasher,
            tenant.Id,
            logger,
            cancellationToken);
    }

    private static async Task EnsureTenantPasswordEncryptedAsync(
        PlatformDbContext platformDb,
        Tenant tenant,
        ITenantDbCredentialProtector credentialProtector,
        CancellationToken cancellationToken)
    {
        var plain = credentialProtector.Unprotect(tenant.DbPassword);
        var encrypted = credentialProtector.Protect(plain);
        if (string.Equals(tenant.DbPassword, encrypted, StringComparison.Ordinal))
        {
            return;
        }

        tenant.DbPassword = encrypted;
        await platformDb.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPlatformPermissionsAsync(
        PlatformDbContext platformDb,
        CancellationToken cancellationToken)
    {
        var existing = await platformDb.Permissions.Select(p => p.Code).ToListAsync(cancellationToken);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);

        foreach (var def in PermissionCatalog.All)
        {
            if (existingSet.Contains(def.Code))
            {
                continue;
            }

            platformDb.Permissions.Add(new Permission
            {
                Code = def.Code,
                Name = def.Name,
                Module = def.Module,
                IsSystem = true
            });
        }

        await platformDb.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTenantAdminAsync(
        ITenantDbContextFactory tenantDbFactory,
        IPasswordHasher passwordHasher,
        Guid tenantId,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var tenantDb = await tenantDbFactory.CreateAsync(tenantId, cancellationToken);

        var tenantContext = new TenantContext { TenantId = tenantId };
        // Interceptor uses accessor from DI in request scope; for seeder set tenant id on entities explicitly.

        var role = await tenantDb.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == tenantId && r.Name == TenantAdminRoleName,
                cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                TenantId = tenantId,
                Name = TenantAdminRoleName,
                Description = "Full tenant administrator",
                IsSystem = true
            };
            tenantDb.Roles.Add(role);
            await tenantDb.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created {Role} in tenant database {TenantId}", TenantAdminRoleName, tenantId);
        }

        await GrantAllPermissionsToRoleAsync(tenantDb, tenantId, role.Id, cancellationToken);

        var normalizedEmail = DefaultAdminEmail.ToLowerInvariant();
        var existingUser = await tenantDb.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            existingUser.PasswordHash = passwordHasher.Hash(DefaultAdminPassword);
            existingUser.Status = UserStatus.Active;
            await tenantDb.SaveChangesAsync(cancellationToken);
            return;
        }

        var user = new User
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
            Status = UserStatus.Active,
            EmailConfirmed = true
        };

        tenantDb.Users.Add(user);
        tenantDb.UserProfiles.Add(new UserProfile
        {
            TenantId = tenantId,
            UserId = user.Id,
            FirstName = "Demo",
            LastName = "Admin"
        });
        tenantDb.UserRoles.Add(new UserRole
        {
            TenantId = tenantId,
            UserId = user.Id,
            RoleId = role.Id
        });

        await tenantDb.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created bootstrap admin {Email} in tenant database {TenantId}", normalizedEmail, tenantId);
    }

    private static async Task SyncTenantAdminRolePermissionsAsync(
        ITenantDbContextFactory tenantDbFactory,
        IPermissionResolver permissionResolver,
        Guid tenantId,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var tenantDb = await tenantDbFactory.CreateAsync(tenantId, cancellationToken);

        var role = await tenantDb.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == tenantId && r.Name == TenantAdminRoleName,
                cancellationToken);

        if (role is null)
        {
            return;
        }

        var added = await GrantAllPermissionsToRoleAsync(tenantDb, tenantId, role.Id, cancellationToken);
        if (added > 0)
        {
            logger.LogInformation(
                "Granted {Count} new permission(s) to {Role} in tenant {TenantId}",
                added,
                TenantAdminRoleName,
                tenantId);
        }

        var userIds = await tenantDb.UserRoles
            .IgnoreQueryFilters()
            .Where(ur => ur.TenantId == tenantId && ur.RoleId == role.Id && !ur.IsDeleted)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
        {
            await permissionResolver.InvalidateUserPermissionsCacheAsync(
                userId,
                tenantId,
                cancellationToken);
        }
    }

    private static async Task<int> GrantAllPermissionsToRoleAsync(
        ApplicationDbContext tenantDb,
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var allPermissionIds = await tenantDb.Permissions
            .IgnoreQueryFilters()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var assignedIds = await tenantDb.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.TenantId == tenantId && rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var assignedSet = assignedIds.ToHashSet();
        var added = 0;

        foreach (var permissionId in allPermissionIds)
        {
            if (assignedSet.Contains(permissionId))
            {
                continue;
            }

            tenantDb.RolePermissions.Add(new RolePermission
            {
                TenantId = tenantId,
                RoleId = roleId,
                PermissionId = permissionId
            });
            added++;
        }

        if (added > 0)
        {
            await tenantDb.SaveChangesAsync(cancellationToken);
        }

        return added;
    }
}
