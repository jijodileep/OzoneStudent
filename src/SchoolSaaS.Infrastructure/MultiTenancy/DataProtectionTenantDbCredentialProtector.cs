using Microsoft.AspNetCore.DataProtection;
using SchoolSaaS.Application.Abstractions.MultiTenancy;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed class DataProtectionTenantDbCredentialProtector(IDataProtectionProvider dataProtectionProvider)
    : ITenantDbCredentialProtector
{
    private const string ProtectorPurpose = "SchoolSaaS.TenantDbPassword.v1";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    public string Protect(string plainTextPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainTextPassword);
        return _protector.Protect(plainTextPassword);
    }

    public string Unprotect(string protectedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedPassword);

        try
        {
            return _protector.Unprotect(protectedPassword);
        }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException
                                   or FormatException)
        {
            // Legacy rows may still hold plaintext until re-saved.
            return protectedPassword;
        }
    }
}
