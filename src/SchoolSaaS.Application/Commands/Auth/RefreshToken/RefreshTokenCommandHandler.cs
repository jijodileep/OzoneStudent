using MediatR;
using SchoolSaaS.Application.Commands.Auth.Login;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IPermissionResolver permissionResolver,
    IAuditService auditService) : IRequestHandler<RefreshTokenCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await refreshTokenService.ValidateAsync(request.RefreshToken, cancellationToken);
        if (!validation.IsValid || validation.UserId is null || validation.TenantId is null)
        {
            return Result<LoginResult>.Failure(
                "auth.invalid_refresh_token",
                validation.FailureReason ?? "Invalid or expired refresh token.");
        }

        var user = await userRepository.GetByIdAsync(
            validation.TenantId.Value,
            validation.UserId.Value,
            cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            return Result<LoginResult>.Failure(
                "auth.invalid_refresh_token",
                "User is not available.");
        }

        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);

        var roles = await permissionResolver.GetRoleNamesAsync(user.Id, user.TenantId, cancellationToken);
        var permissions = await permissionResolver.GetPermissionsAsync(user.Id, user.TenantId, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(
            new JwtUserContext(user.Id, user.TenantId, user.Email, roles, permissions));

        var refresh = await refreshTokenService.IssueAsync(
            user.Id,
            user.TenantId,
            ipAddress: null,
            cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.TokenRefresh,
                AuditCategories.Auth,
                EntityType: nameof(User),
                EntityId: user.Id,
                Description: $"Refresh token rotated for {user.Email}"),
            cancellationToken);

        return Result<LoginResult>.Success(new LoginResult(
            accessToken,
            jwtTokenService.GetAccessTokenExpiry(),
            refresh.Token,
            refresh.ExpiresAt,
            user.Id,
            user.Email));
    }
}

