using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand<bool>, IAllowAnonymousCommand;

