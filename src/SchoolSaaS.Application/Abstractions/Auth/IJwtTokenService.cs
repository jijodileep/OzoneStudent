namespace SchoolSaaS.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(JwtUserContext user);

    DateTime GetAccessTokenExpiry();
}

public sealed record JwtUserContext(
    Guid UserId,
    Guid TenantId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
