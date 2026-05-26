using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Users.InviteUser;

[RequirePermission(PermissionCodes.UsersInvite)]
public sealed record InviteUserCommand(
    string Email,
    string RoleName,
    string? FirstName = null,
    string? LastName = null) : ICommand<InviteUserResult>;

public sealed record InviteUserResult(
    Guid InvitationId,
    string Email,
    DateTime ExpiresAt,
    string? InvitationToken = null);

