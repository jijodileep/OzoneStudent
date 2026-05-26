using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Infrastructure.Identity;

public static class DefaultSystemRolesSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext tenantDb,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var existingNames = await tenantDb.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId)
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        var existingSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, description) in DefaultSystemRoles.All)
        {
            if (existingSet.Contains(name))
            {
                continue;
            }

            tenantDb.Roles.Add(new Role
            {
                TenantId = tenantId,
                Name = name,
                Description = description,
                IsSystem = true
            });
        }

        await tenantDb.SaveChangesAsync(cancellationToken);
    }
}
