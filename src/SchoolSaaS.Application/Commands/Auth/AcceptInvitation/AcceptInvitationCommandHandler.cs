using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Auth.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler(
    IUserInvitationRepository invitationRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    ISecureTokenGenerator secureTokenGenerator,
    IAuditService auditService) : IRequestHandler<AcceptInvitationCommand, Result<AcceptInvitationResult>>
{
    public async Task<Result<AcceptInvitationResult>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = secureTokenGenerator.HashToken(request.Token.Trim());
        var invitation = await invitationRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken);

        if (invitation is null)
        {
            return Result<AcceptInvitationResult>.Failure(
                "auth.invalid_token",
                "Invalid or expired invitation token.");
        }

        if (await userRepository.EmailExistsAsync(invitation.TenantId, invitation.Email, cancellationToken))
        {
            return Result<AcceptInvitationResult>.Conflict("A user with this email already exists.");
        }

        var user = new User
        {
            TenantId = invitation.TenantId,
            Email = invitation.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Status = UserStatus.Active,
            EmailConfirmed = true,
            CreatedBy = invitation.InvitedByUserId
        };

        var profile = new UserProfile
        {
            TenantId = invitation.TenantId,
            UserId = user.Id,
            FirstName = string.IsNullOrWhiteSpace(invitation.FirstName) ? "User" : invitation.FirstName,
            LastName = string.IsNullOrWhiteSpace(invitation.LastName) ? "Invited" : invitation.LastName,
            CreatedBy = invitation.InvitedByUserId
        };

        await userRepository.AddAsync(user, profile, cancellationToken);
        await roleRepository.AssignRoleToUserAsync(
            invitation.TenantId,
            user.Id,
            invitation.RoleId,
            invitation.InvitedByUserId,
            cancellationToken);

        await invitationRepository.MarkAcceptedAsync(invitation.Id, cancellationToken);

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.InvitationAccepted,
                AuditCategories.Auth,
                EntityType: nameof(User),
                EntityId: user.Id,
                Description: $"Invitation accepted for {invitation.Email}"),
            cancellationToken);

        return Result<AcceptInvitationResult>.Success(new AcceptInvitationResult(user.Id, user.Email));
    }
}

