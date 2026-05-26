using System.Security.Cryptography;
using System.Text;

namespace SchoolSaaS.Infrastructure.Identity;

internal static class SecureTokenGenerator
{
    public static string GeneratePlainToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes);
    }
}
