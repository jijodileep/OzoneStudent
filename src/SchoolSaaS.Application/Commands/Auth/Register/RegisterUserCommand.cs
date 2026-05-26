using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Auth.Register;

[RequirePermission(PermissionCodes.UsersManage)]
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string RoleName = "tenant_admin") : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId, string Email);

