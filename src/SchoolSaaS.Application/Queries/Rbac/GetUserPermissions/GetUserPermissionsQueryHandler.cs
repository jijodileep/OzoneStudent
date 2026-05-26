using MediatR;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Rbac.GetUserPermissions;

public sealed class GetUserPermissionsQueryHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IPermissionResolver permissionResolver,
    IScopeResolver scopeResolver) : IRequestHandler<GetUserPermissionsQuery, Result<GetUserPermissionsResult>>
{
    public async Task<Result<GetUserPermissionsResult>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is null)
        {
            return Result<GetUserPermissionsResult>.Failure(
                "rbac.unauthorized",
                "Authentication is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var userId = request.UserId ?? tenantContext.UserId.Value;

        if (userId != tenantContext.UserId.Value)
        {
            var canManage = await permissionResolver.HasPermissionAsync(
                tenantContext.UserId.Value,
                tenantId,
                PermissionCodes.UsersManage,
                cancellationToken);

            if (!canManage)
            {
                return Result<GetUserPermissionsResult>.Forbidden(
                    "Missing permission to view other users' permissions.");
            }
        }

        var user = await userRepository.GetByIdAsync(tenantId, userId, cancellationToken);
        if (user is null)
        {
            return Result<GetUserPermissionsResult>.NotFound("User not found.");
        }

        var roles = await permissionResolver.GetRoleNamesAsync(userId, tenantId, cancellationToken);
        var permissions = await permissionResolver.GetPermissionsAsync(userId, tenantId, cancellationToken);
        var scope = await scopeResolver.ResolveAsync(userId, tenantId, cancellationToken);

        return Result<GetUserPermissionsResult>.Success(new GetUserPermissionsResult(
            userId,
            roles,
            permissions,
            scope.ScopeType.ToString(),
            scope.ScopeIds));
    }
}
