using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Interceptors;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.UnitTests.Persistence;

public class TenantSaveChangesInterceptorTests
{
    [Fact]
    public async Task AddedTenantIsolationProbe_GetsTenantIdStamped()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext { TenantId = tenantId };
        var accessor = new TenantContextAccessor(tenantContext);

        var interceptor = new TenantSaveChangesInterceptor(accessor);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new ApplicationDbContext(options, accessor);
        context.TenantIsolationProbes.Add(new TenantIsolationProbe { Label = "probe" });

        await context.SaveChangesAsync();

        var saved = await context.TenantIsolationProbes.IgnoreQueryFilters().FirstAsync();
        saved.TenantId.Should().Be(tenantId);
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
