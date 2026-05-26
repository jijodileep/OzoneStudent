using FluentAssertions;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.UnitTests.MultiTenancy;

public class TenantContextTests
{
    [Fact]
    public void DefaultContext_HasNullTenantId()
    {
        var context = new TenantContext();

        context.TenantId.Should().BeNull();
        context.BranchId.Should().BeNull();
        context.UserId.Should().BeNull();
        context.IsAuthenticated.Should().BeFalse();
        context.IsSuperAdmin.Should().BeFalse();
    }
}
