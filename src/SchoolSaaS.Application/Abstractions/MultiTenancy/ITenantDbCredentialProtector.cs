namespace SchoolSaaS.Application.Abstractions.MultiTenancy;

/// <summary>
/// Encrypts and decrypts tenant database passwords stored in the platform catalog.
/// </summary>
public interface ITenantDbCredentialProtector
{
    string Protect(string plainTextPassword);

    string Unprotect(string protectedPassword);
}
