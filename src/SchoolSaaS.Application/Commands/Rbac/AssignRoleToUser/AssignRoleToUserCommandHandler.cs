using MediatR;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Rbac.AssignRoleToUser;

public sealed class AssignRoleToUserCommandHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPermissionResolver permissionResolver) : IRequestHandler<AssignRoleToUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        AssignRoleToUserCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<bool>.Failure("rbac.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;

        var user = await userRepository.GetByIdAsync(tenantId, request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<bool>.NotFound("User not found.");
        }

        var role = await roleRepository.GetByIdAsync(tenantId, request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result<bool>.NotFound("Role not found.");
        }

        await roleRepository.AssignRoleToUserAsync(
            tenantId,
            request.UserId,
            request.RoleId,
            tenantContext.UserId,
            cancellationToken);

        await permissionResolver.InvalidateUserPermissionsCacheAsync(
            request.UserId,
            tenantId,
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
