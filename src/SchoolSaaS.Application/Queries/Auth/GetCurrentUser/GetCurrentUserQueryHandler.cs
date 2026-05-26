using MediatR;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Auth.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IPermissionResolver permissionResolver) : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    public async Task<Result<CurrentUserDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is null)
        {
            return Result<CurrentUserDto>.Failure(
                "auth.unauthorized",
                "Authentication is required.");
        }

        var user = await userRepository.GetByIdAsync(
            tenantContext.TenantId.Value,
            tenantContext.UserId.Value,
            cancellationToken);

        if (user is null)
        {
            return Result<CurrentUserDto>.NotFound("User not found.");
        }

        var profile = await userRepository.GetProfileAsync(
            tenantContext.TenantId.Value,
            user.Id,
            cancellationToken);

        var roles = await permissionResolver.GetRoleNamesAsync(user.Id, user.TenantId, cancellationToken);
        var permissions = await permissionResolver.GetPermissionsAsync(user.Id, user.TenantId, cancellationToken);

        return Result<CurrentUserDto>.Success(new CurrentUserDto(
            user.Id,
            user.TenantId,
            user.Email,
            profile?.FirstName ?? string.Empty,
            profile?.LastName ?? string.Empty,
            roles,
            permissions));
    }
}

