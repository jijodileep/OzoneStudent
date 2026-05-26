using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Domain.Exceptions;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.IntegrationTests.Tenancy;

public sealed class TenantIsolationIntegrationTests : IntegrationTestBase, IAsyncLifetime
{
    private Guid _tenantAId;
    private Guid _tenantBId;
    private Guid _tenantBProbeId;

    public TenantIsolationIntegrationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    public async Task InitializeAsync() => await SeedTenantsAndProbesAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Query_AsTenantA_ReturnsOnlyTenantAProbes()
    {
        var probes = await RunAsTenantAsync(_tenantAId, db =>
            db.TenantIsolationProbes.Select(p => p.Label).ToListAsync());

        probes.Should().BeEquivalentTo(["tenant-a-probe"]);
        probes.Should().NotContain("tenant-b-probe");
    }

    [Fact]
    public async Task Query_AsTenantA_DoesNotReturnTenantBProbeById()
    {
        var probe = await RunAsTenantAsync(_tenantAId, db =>
            db.TenantIsolationProbes
                .Where(p => p.Id == _tenantBProbeId)
                .SingleOrDefaultAsync());

        probe.Should().BeNull();
    }

    [Fact]
    public async Task Update_AsTenantA_OnTenantBProbe_ThrowsCrossTenantWriteException()
    {
        var act = async () => await RunAsTenantAsync(_tenantAId, async db =>
        {
            var probe = await db.TenantIsolationProbes
                .IgnoreQueryFilters()
                .SingleAsync(p => p.Id == _tenantBProbeId);

            probe.Label = "hijacked";
            await db.SaveChangesAsync();
        });

        await act.Should().ThrowAsync<CrossTenantWriteException>();
    }

    [Fact]
    public async Task Insert_WithWrongTenantId_ThrowsCrossTenantWriteException()
    {
        var act = async () => await RunAsTenantAsync(_tenantAId, async db =>
        {
            db.TenantIsolationProbes.Add(new TenantIsolationProbe
            {
                TenantId = _tenantBId,
                Label = "cross-tenant-insert"
            });

            await db.SaveChangesAsync();
        });

        await act.Should().ThrowAsync<CrossTenantWriteException>();
    }

    private async Task SeedTenantsAndProbesAsync()
    {
        _tenantAId = Guid.NewGuid();
        _tenantBId = Guid.NewGuid();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Tenants.AddRange(
                CreateTenant(_tenantAId, "Tenant A", $"tenant-a-{_tenantAId:N}"),
                CreateTenant(_tenantBId, "Tenant B", $"tenant-b-{_tenantBId:N}"));

            await db.SaveChangesAsync();
        }

        await RunAsTenantAsync(_tenantAId, async db =>
        {
            db.TenantIsolationProbes.Add(new TenantIsolationProbe { Label = "tenant-a-probe" });
            await db.SaveChangesAsync();
        });

        await RunAsTenantAsync(_tenantBId, async db =>
        {
            var probe = new TenantIsolationProbe { Label = "tenant-b-probe" };
            db.TenantIsolationProbes.Add(probe);
            await db.SaveChangesAsync();
            _tenantBProbeId = probe.Id;
        });
    }

    private static Tenant CreateTenant(Guid id, string name, string slug) => new()
    {
        Id = id,
        Name = name,
        Slug = slug,
        DbServer = "localhost",
        DbPort = 3306,
        DbName = slug,
        DbUser = "root",
        DbPassword = "password",
        Plan = "free",
        Status = TenantStatus.Active
    };

    private async Task<T> RunAsTenantAsync<T>(Guid tenantId, Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        tenantContext.TenantId = tenantId;

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(db);
    }

    private Task RunAsTenantAsync(Guid tenantId, Func<ApplicationDbContext, Task> action) =>
        RunAsTenantAsync(tenantId, async db =>
        {
            await action(db);
            return true;
        });
}
