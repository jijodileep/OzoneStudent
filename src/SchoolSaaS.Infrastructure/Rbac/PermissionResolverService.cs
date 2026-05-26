using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Infrastructure.MultiTenancy;
using System.Text.Json;

namespace SchoolSaaS.Infrastructure.Rbac;

public sealed class PermissionResolverService(
    ITenantDbContextFactory tenantDbFactory,
    IDistributedCache cache) : IPermissionResolver
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"permissions:{tenantId:N}:{userId:N}";
        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<string>>(cached) ?? [];
        }

        await using var db = await tenantDbFactory.CreateAsync(tenantId, cancellationToken);

        var permissions = await (
            from ur in db.UserRoles.IgnoreQueryFilters().AsNoTracking()
            join rp in db.RolePermissions.IgnoreQueryFilters().AsNoTracking() on ur.RoleId equals rp.RoleId
            join p in db.Permissions.IgnoreQueryFilters().AsNoTracking() on rp.PermissionId equals p.Id
            where !ur.IsDeleted && ur.TenantId == tenantId && ur.UserId == userId
            select p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(permissions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
            cancellationToken);

        return permissions;
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await tenantDbFactory.CreateAsync(tenantId, cancellationToken);

        return await (
            from ur in db.UserRoles.IgnoreQueryFilters().AsNoTracking()
            join r in db.Roles.IgnoreQueryFilters().AsNoTracking() on ur.RoleId equals r.Id
            where !ur.IsDeleted && !r.IsDeleted && ur.TenantId == tenantId && ur.UserId == userId
            select r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, tenantId, cancellationToken);
        return permissions.Contains(permission, StringComparer.Ordinal);
    }

    public Task InvalidateUserPermissionsCacheAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"permissions:{tenantId:N}:{userId:N}";
        return cache.RemoveAsync(cacheKey, cancellationToken);
    }
}
