using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IPasswordResetTokenRepository resetTokenRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator secureTokenGenerator,
    IRefreshTokenService refreshTokenService,
    IAuditService auditService) : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = secureTokenGenerator.HashToken(request.Token.Trim());
        var stored = await resetTokenRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null)
        {
            return Result<bool>.Failure(
                "auth.invalid_token",
                "Invalid or expired reset token.");
        }

        await userRepository.UpdatePasswordAsync(
            stored.TenantId,
            stored.UserId,
            passwordHasher.Hash(request.NewPassword),
            stored.UserId,
            cancellationToken);

        await resetTokenRepository.MarkUsedAsync(stored.Id, cancellationToken);
        await refreshTokenService.RevokeAllForUserAsync(stored.UserId, stored.TenantId, cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.PasswordResetComplete,
                AuditCategories.Auth,
                EntityType: nameof(User),
                EntityId: stored.UserId,
                Description: "Password reset completed"),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}

