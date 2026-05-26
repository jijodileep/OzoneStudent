using FluentAssertions;
using SchoolSaaS.Infrastructure.Caching;

namespace SchoolSaaS.UnitTests.Caching;

public class RedisCacheKeysTests
{
    [Fact]
    public void Build_SameLogicalKey_DifferentTenants_ProduceDifferentKeys()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var keyA = RedisCacheKeys.Build("schoolsaas:dev", tenantA, "profile");
        var keyB = RedisCacheKeys.Build("schoolsaas:dev", tenantB, "profile");

        keyA.Should().NotBe(keyB);
        keyA.Should().Contain(tenantA.ToString());
        keyB.Should().Contain(tenantB.ToString());
    }
}
