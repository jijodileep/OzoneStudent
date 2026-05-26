using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.AcceptInvitation;

public sealed record AcceptInvitationCommand(string Token, string Password) : ICommand<AcceptInvitationResult>;

public sealed record AcceptInvitationResult(Guid UserId, string Email);

