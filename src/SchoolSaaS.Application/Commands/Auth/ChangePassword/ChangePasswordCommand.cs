using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Auth.ChangePassword;

[RequirePermission(PermissionCodes.AuthPasswordChange)]
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<bool>;

