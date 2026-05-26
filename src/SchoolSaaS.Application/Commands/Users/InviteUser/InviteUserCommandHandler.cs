using MediatR;
using Microsoft.Extensions.Configuration;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Notifications;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Users.InviteUser;

public sealed class InviteUserCommandHandler(
    ITenantContext tenantContext,
    IUserRepository userRepository,
    IUserInvitationRepository invitationRepository,
    IRoleRepository roleRepository,
    ISecureTokenGenerator secureTokenGenerator,
    IEmailSender emailSender,
    IAuditService auditService,
    IAppUrlProvider appUrlProvider,
    IConfiguration configuration) : IRequestHandler<InviteUserCommand, Result<InviteUserResult>>
{
    public async Task<Result<InviteUserResult>> Handle(
        InviteUserCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is null)
        {
            return Result<InviteUserResult>.Failure("auth.unauthorized", "Authentication is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await userRepository.EmailExistsAsync(tenantId, normalizedEmail, cancellationToken))
        {
            return Result<InviteUserResult>.Conflict("A user with this email already exists.");
        }

        if (await invitationRepository.HasPendingInvitationAsync(tenantId, normalizedEmail, cancellationToken))
        {
            return Result<InviteUserResult>.Conflict("A pending invitation already exists for this email.");
        }

        var role = await roleRepository.FindByNameAsync(tenantId, request.RoleName, cancellationToken);
        if (role is null)
        {
            return Result<InviteUserResult>.Failure("roles.not_found", $"Role '{request.RoleName}' was not found.");
        }

        var plainToken = secureTokenGenerator.GeneratePlainToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var invitation = new UserInvitation
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            RoleId = role.Id,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            TokenHash = secureTokenGenerator.HashToken(plainToken),
            ExpiresAt = expiresAt,
            InvitedByUserId = tenantContext.UserId.Value,
            CreatedBy = tenantContext.UserId
        };

        await invitationRepository.CreateAsync(invitation, cancellationToken);

        var inviteUrl =
            $"{appUrlProvider.GetPublicBaseUrl()}/api/v1/auth/accept-invitation?token={Uri.EscapeDataString(plainToken)}";
        await emailSender.SendUserInvitationAsync(normalizedEmail, inviteUrl, cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.UserInvited,
                AuditCategories.Auth,
                EntityType: nameof(UserInvitation),
                EntityId: invitation.Id,
                Description: $"Invitation sent to {normalizedEmail}"),
            cancellationToken);

        var exposeToken = configuration.GetValue<bool>("Testing:ExposeInvitationTokens");

        return Result<InviteUserResult>.Success(
            new InviteUserResult(
                invitation.Id,
                invitation.Email,
                invitation.ExpiresAt,
                exposeToken ? plainToken : null));
    }
}

