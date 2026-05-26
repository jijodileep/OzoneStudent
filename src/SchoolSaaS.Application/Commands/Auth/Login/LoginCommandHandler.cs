using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITenantContext tenantContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IPermissionResolver permissionResolver,
    IAuditService auditService) : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<LoginResult>.Failure(
                "auth.tenant_required",
                "Tenant context is required. Use your school subdomain or X-Tenant-Slug header.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await userRepository.FindByEmailAsync(tenantId, normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await auditService.LogAsync(
                new AuditEntry(
                    AuditActions.LoginFailed,
                    AuditCategories.Auth,
                    Description: $"Failed login for {normalizedEmail}",
                    Outcome: AuditOutcomes.Failed),
                cancellationToken);

            return Result<LoginResult>.Failure(
                "auth.invalid_credentials",
                "Invalid email or password.");
        }

        if (user.Status == UserStatus.Locked)
        {
            return Result<LoginResult>.Failure(
                "auth.account_locked",
                "Account is locked. Contact your administrator.");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<LoginResult>.Failure(
                "auth.account_inactive",
                "Account is not active.");
        }

        var roles = await permissionResolver.GetRoleNamesAsync(user.Id, tenantId, cancellationToken);
        var permissions = await permissionResolver.GetPermissionsAsync(user.Id, tenantId, cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(
            new JwtUserContext(user.Id, tenantId, user.Email, roles, permissions));

        var refresh = await refreshTokenService.IssueAsync(
            user.Id,
            tenantId,
            ipAddress: null,
            cancellationToken);

        await userRepository.UpdateLastLoginAsync(user.Id, cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.Login,
                AuditCategories.Auth,
                EntityType: nameof(User),
                EntityId: user.Id,
                Description: $"User {user.Email} logged in",
                Outcome: AuditOutcomes.Success),
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

