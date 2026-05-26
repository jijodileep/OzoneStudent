using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<bool>, IAllowAnonymousCommand;

