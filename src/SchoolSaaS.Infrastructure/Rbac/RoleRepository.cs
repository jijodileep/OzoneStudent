using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Rbac;

public sealed class RoleRepository(ApplicationDbContext db) : IRoleRepository
{
    public Task<Role?> FindByNameAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default) =>
        db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.TenantId == tenantId && r.Name == name,
                cancellationToken);

    public Task<Role?> GetByIdAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        db.Roles.FirstOrDefaultAsync(
            r => r.TenantId == tenantId && r.Id == roleId,
            cancellationToken);

    public async Task<IReadOnlyList<Role>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await db.Roles.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

    public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        db.Roles.Add(role);
        await db.SaveChangesAsync(cancellationToken);
        return role;
    }

    public async Task AssignRoleToUserAsync(
        Guid tenantId,
        Guid userId,
        Guid roleId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.UserRoles.AnyAsync(
            ur => ur.TenantId == tenantId && ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted,
            cancellationToken);

        if (exists)
        {
            return;
        }

        db.UserRoles.Add(new UserRole
        {
            TenantId = tenantId,
            UserId = userId,
            RoleId = roleId,
            AssignedBy = assignedBy
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceRolePermissionsAsync(
        Guid tenantId,
        Guid roleId,
        IReadOnlyList<string> permissionCodes,
        Guid? grantedBy,
        CancellationToken cancellationToken = default)
    {
        var permissions = await db.Permissions
            .IgnoreQueryFilters()
            .Where(p => permissionCodes.Contains(p.Code))
            .ToListAsync(cancellationToken);

        var existing = await db.RolePermissions
            .Where(rp => rp.TenantId == tenantId && rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        db.RolePermissions.RemoveRange(existing);

        foreach (var permission in permissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                TenantId = tenantId,
                RoleId = roleId,
                PermissionId = permission.Id,
                GrantedBy = grantedBy,
                GrantedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesForRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await (
            from rp in db.RolePermissions.AsNoTracking()
            join p in db.Permissions.IgnoreQueryFilters().AsNoTracking() on rp.PermissionId equals p.Id
            where rp.TenantId == tenantId && rp.RoleId == roleId && !rp.IsDeleted
            select p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetUserIdsForRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await db.UserRoles.AsNoTracking()
            .Where(ur => ur.TenantId == tenantId && ur.RoleId == roleId && !ur.IsDeleted)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);
}
