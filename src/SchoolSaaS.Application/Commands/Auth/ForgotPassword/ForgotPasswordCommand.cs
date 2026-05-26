using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<ForgotPasswordResult>, IAllowAnonymousCommand;

public sealed record ForgotPasswordResult(string Message, string? ResetToken = null);

