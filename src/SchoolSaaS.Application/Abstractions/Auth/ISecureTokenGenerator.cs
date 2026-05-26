namespace SchoolSaaS.Application.Abstractions.Auth;

public interface ISecureTokenGenerator
{
    string GeneratePlainToken();

    string HashToken(string plainToken);
}
