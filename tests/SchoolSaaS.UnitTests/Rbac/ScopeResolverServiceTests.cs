using FluentAssertions;
using NSubstitute;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Infrastructure.Rbac;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.UnitTests.Rbac;

public sealed class ScopeResolverServiceTests
{
    [Fact]
    public async Task Resolve_TenantAdmin_ReturnsTenantScope()
    {
        var permissionResolver = Substitute.For<IPermissionResolver>();
        permissionResolver
            .GetRoleNamesAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([DefaultSystemRoles.TenantAdmin]);

        var resolver = new ScopeResolverService(permissionResolver);
        var scope = await resolver.ResolveAsync(Guid.NewGuid(), Guid.NewGuid());

        scope.ScopeType.Should().Be(ScopeType.Tenant);
        scope.ScopeIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Resolve_Parent_ReturnsSelfScopeWithUserId()
    {
        var userId = Guid.NewGuid();
        var permissionResolver = Substitute.For<IPermissionResolver>();
        permissionResolver
            .GetRoleNamesAsync(userId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([DefaultSystemRoles.Parent]);

        var resolver = new ScopeResolverService(permissionResolver);
        var scope = await resolver.ResolveAsync(userId, Guid.NewGuid());

        scope.ScopeType.Should().Be(ScopeType.Self);
        scope.ScopeIds.Should().Equal([userId]);
    }
}
