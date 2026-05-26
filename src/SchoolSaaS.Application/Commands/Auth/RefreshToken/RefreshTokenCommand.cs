using SchoolSaaS.Application.Commands.Auth.Login;
using SchoolSaaS.Application.Common;

namespace SchoolSaaS.Application.Commands.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResult>;

