using MediatR;
using Microsoft.Extensions.Configuration;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Notifications;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IPasswordResetTokenRepository resetTokenRepository,
    ISecureTokenGenerator secureTokenGenerator,
    IEmailSender emailSender,
    IAuditService auditService,
    IAppUrlProvider appUrlProvider,
    IConfiguration configuration) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>>
{
    private const string GenericMessage =
        "If an account exists for this email, password reset instructions have been sent.";

    public async Task<Result<ForgotPasswordResult>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ForgotPasswordResult>.Failure(
                "auth.tenant_required",
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.FindByEmailAsync(tenantId, normalizedEmail, cancellationToken);
        string? exposedToken = null;
        var exposeToken = configuration.GetValue<bool>("Testing:ExposeResetTokens");

        if (user is not null && user.Status == UserStatus.Active)
        {
            await resetTokenRepository.InvalidateActiveForUserAsync(tenantId, user.Id, cancellationToken);

            var plainToken = secureTokenGenerator.GeneratePlainToken();
            var token = new PasswordResetToken
            {
                TenantId = tenantId,
                UserId = user.Id,
                TokenHash = secureTokenGenerator.HashToken(plainToken),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await resetTokenRepository.CreateAsync(token, cancellationToken);

            var resetUrl =
                $"{appUrlProvider.GetPublicBaseUrl()}/api/v1/auth/reset-password?token={Uri.EscapeDataString(plainToken)}";
            await emailSender.SendPasswordResetAsync(user.Email, resetUrl, cancellationToken);

            await auditService.LogAsync(
                new AuditEntry(
                    AuditActions.PasswordResetRequest,
                    AuditCategories.Auth,
                    EntityType: nameof(User),
                    EntityId: user.Id,
                    Description: $"Password reset requested for {normalizedEmail}"),
                cancellationToken);

            if (exposeToken)
            {
                exposedToken = plainToken;
            }
        }

        return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(GenericMessage, exposedToken));
    }
}

