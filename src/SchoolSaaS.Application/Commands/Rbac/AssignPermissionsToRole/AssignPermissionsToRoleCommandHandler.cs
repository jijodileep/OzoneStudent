using MediatR;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Rbac.AssignPermissionsToRole;

public sealed class AssignPermissionsToRoleCommandHandler(
    ITenantContext tenantContext,
    IRoleRepository roleRepository,
    IPermissionResolver permissionResolver) : IRequestHandler<AssignPermissionsToRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        AssignPermissionsToRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<bool>.Failure("rbac.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var role = await roleRepository.GetByIdAsync(tenantId, request.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<bool>.NotFound("Role not found.");
        }

        if (role.IsSystem && string.Equals(role.Name, DefaultSystemRoles.TenantAdmin, StringComparison.Ordinal))
        {
            return Result<bool>.Failure(
                "rbac.system_role_protected",
                "Cannot modify permissions on the tenant administrator role.");
        }

        await roleRepository.ReplaceRolePermissionsAsync(
            tenantId,
            role.Id,
            request.Codes,
            tenantContext.UserId,
            cancellationToken);

        var userIds = await roleRepository.GetUserIdsForRoleAsync(tenantId, role.Id, cancellationToken);
        foreach (var userId in userIds)
        {
            await permissionResolver.InvalidateUserPermissionsCacheAsync(
                userId,
                tenantId,
                cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
