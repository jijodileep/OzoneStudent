using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService) : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is null)
        {
            return Result<bool>.Failure("auth.unauthorized", "Authentication is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var userId = tenantContext.UserId.Value;

        var user = await userRepository.GetByIdAsync(tenantId, userId, cancellationToken);
        if (user is null)
        {
            return Result<bool>.NotFound("User not found.");
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result<bool>.Failure("auth.invalid_credentials", "Current password is incorrect.");
        }

        await userRepository.UpdatePasswordAsync(
            tenantId,
            userId,
            passwordHasher.Hash(request.NewPassword),
            userId,
            cancellationToken);

        await refreshTokenService.RevokeAllForUserAsync(userId, tenantId, cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.PasswordChange,
                AuditCategories.Auth,
                EntityType: nameof(User),
                EntityId: userId,
                Description: "Password changed"),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}

