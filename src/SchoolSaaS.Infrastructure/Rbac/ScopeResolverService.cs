using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Infrastructure.Rbac;

public sealed class ScopeResolverService(IPermissionResolver permissionResolver) : IScopeResolver
{
    private static readonly HashSet<string> TenantWideRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        DefaultSystemRoles.TenantAdmin,
        "super_admin"
    };

    private static readonly HashSet<string> ClassScopedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        DefaultSystemRoles.Teacher
    };

    private static readonly HashSet<string> SelfScopedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        DefaultSystemRoles.Parent,
        DefaultSystemRoles.Student
    };

    public async Task<UserAccessScope> ResolveAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var roles = await permissionResolver.GetRoleNamesAsync(userId, tenantId, cancellationToken);

        if (roles.Any(r => TenantWideRoles.Contains(r)))
        {
            return new UserAccessScope(ScopeType.Tenant, []);
        }

        if (roles.Any(r => ClassScopedRoles.Contains(r)))
        {
            // Class assignments are added in Sprint 3+ (AssignClassTeacherCommand).
            return new UserAccessScope(ScopeType.Class, []);
        }

        if (roles.Any(r => SelfScopedRoles.Contains(r)))
        {
            return new UserAccessScope(ScopeType.Self, [userId]);
        }

        return new UserAccessScope(ScopeType.Tenant, []);
    }
}
