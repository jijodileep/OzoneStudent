using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using SchoolSaaS.Infrastructure.MultiTenancy;

namespace SchoolSaaS.UnitTests.MultiTenancy;

public sealed class TenantDbCredentialProtectorTests
{
    [Fact]
    public void Protect_then_Unprotect_returns_original_password()
    {
        var protector = new DataProtectionTenantDbCredentialProtector(
            DataProtectionProvider.Create(nameof(TenantDbCredentialProtectorTests)));

        const string plain = "MyS3cretDbP@ss";

        var protectedValue = protector.Protect(plain);
        protectedValue.Should().NotBe(plain);

        protector.Unprotect(protectedValue).Should().Be(plain);
    }
}
