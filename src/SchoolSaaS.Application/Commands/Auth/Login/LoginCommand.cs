using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResult>;

public sealed record LoginResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email);

