using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.MultiTenancy;
using SchoolSaaS.Application.Abstractions.Platform;
using SchoolSaaS.Application.Commands.Tenants.CreateTenant;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Infrastructure.MultiTenancy;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Infrastructure.Platform;

public sealed class TenantOnboardingService(
    PlatformDbContext platformDb,
    ITenantDatabaseProvisioner provisioner,
    ITenantDbCredentialProtector credentialProtector,
    ITenantDbContextFactory tenantDbFactory,
    IPasswordHasher passwordHasher,
    IOptions<TenancyOptions> tenancyOptions) : ITenantOnboardingService
{
    private const string TenantAdminRoleName = "tenant_admin";

    public async Task<Result<CreateTenantResult>> CreateTenantAsync(
        CreateTenantCommand request,
        CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var opts = tenancyOptions.Value;

        var slugExists = await platformDb.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Slug == slug, cancellationToken);

        if (slugExists)
        {
            return Result<CreateTenantResult>.Conflict($"Tenant slug '{slug}' is already registered.");
        }

        var dbName = string.IsNullOrWhiteSpace(request.DbName)
            ? $"{opts.DatabaseNamePrefix}{slug}"
            : request.DbName.Trim();

        var plainPassword = string.IsNullOrWhiteSpace(request.DbPassword)
            ? opts.DefaultDbPassword
            : request.DbPassword;

        var tenant = new Tenant
        {
            Name = request.Name.Trim(),
            Slug = slug,
            DbServer = string.IsNullOrWhiteSpace(request.DbServer) ? opts.DefaultDbServer : request.DbServer.Trim(),
            DbPort = request.DbPort ?? opts.DefaultDbPort,
            DbName = dbName,
            DbUser = string.IsNullOrWhiteSpace(request.DbUser) ? opts.DefaultDbUser : request.DbUser.Trim(),
            DbPassword = credentialProtector.Protect(plainPassword),
            Plan = request.Plan.Trim(),
            Status = TenantStatus.Active
        };

        platformDb.Tenants.Add(tenant);
        await platformDb.SaveChangesAsync(cancellationToken);

        await provisioner.ProvisionAsync(tenant, cancellationToken);
        await provisioner.SeedTenantPermissionsAsync(tenant.Id, cancellationToken);

        Guid? adminUserId = null;
        if (!string.IsNullOrWhiteSpace(request.AdminEmail) && !string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            adminUserId = await SeedTenantAdminAsync(
                tenant.Id,
                request.AdminEmail,
                request.AdminPassword,
                request.AdminFirstName,
                request.AdminLastName,
                cancellationToken);
        }

        return Result<CreateTenantResult>.Success(new CreateTenantResult(
            tenant.Id,
            tenant.Slug,
            tenant.DbName,
            tenant.DbServer,
            adminUserId));
    }

    private async Task<Guid> SeedTenantAdminAsync(
        Guid tenantId,
        string adminEmail,
        string adminPassword,
        string? firstName,
        string? lastName,
        CancellationToken cancellationToken)
    {
        await using var tenantDb = await tenantDbFactory.CreateAsync(tenantId, cancellationToken);

        var role = await tenantDb.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == TenantAdminRoleName, cancellationToken);

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

            var permissionIds = await tenantDb.Permissions
                .IgnoreQueryFilters()
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            foreach (var permissionId in permissionIds)
            {
                tenantDb.RolePermissions.Add(new RolePermission
                {
                    TenantId = tenantId,
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            await tenantDb.SaveChangesAsync(cancellationToken);
        }

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();
        var user = new User
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(adminPassword),
            Status = UserStatus.Active,
            EmailConfirmed = true
        };

        tenantDb.Users.Add(user);
        tenantDb.UserProfiles.Add(new UserProfile
        {
            TenantId = tenantId,
            UserId = user.Id,
            FirstName = string.IsNullOrWhiteSpace(firstName) ? "Admin" : firstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(lastName) ? "User" : lastName.Trim()
        });
        tenantDb.UserRoles.Add(new UserRole
        {
            TenantId = tenantId,
            UserId = user.Id,
            RoleId = role.Id
        });

        await tenantDb.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}
