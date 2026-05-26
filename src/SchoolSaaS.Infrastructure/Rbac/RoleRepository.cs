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

    public async Task AssignRoleToUserAsync(
        Guid tenantId,
        Guid userId,
        Guid roleId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.UserRoles.AnyAsync(
            ur => ur.TenantId == tenantId && ur.UserId == userId && ur.RoleId == roleId,
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
}
