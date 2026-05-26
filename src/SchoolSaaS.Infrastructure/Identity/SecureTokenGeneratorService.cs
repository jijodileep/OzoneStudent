using SchoolSaaS.Application.Abstractions.Auth;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class SecureTokenGeneratorService : ISecureTokenGenerator
{
    public string GeneratePlainToken() => SecureTokenGenerator.GeneratePlainToken();

    public string HashToken(string plainToken) => SecureTokenGenerator.HashToken(plainToken);
}
