using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Infrastructure.Identity;
using SchoolSaaS.Infrastructure.Persistence.Platform;

namespace SchoolSaaS.IntegrationTests.Institution;

public sealed class TenantProvisioningIntegrationTests : IntegrationTestBase
{
    public TenantProvisioningIntegrationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CreateTenant_RegistersPlatformRowAndReturnsCreated()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = IdentityDataSeeder.DefaultAdminEmail, password = IdentityDataSeeder.DefaultAdminPassword });

        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var slug = $"test-{Guid.NewGuid():N}"[..12];
        var create = await client.PostAsJsonAsync(
            "/api/v1/tenants",
            new
            {
                name = "Integration Test School",
                slug,
                plan = "free",
                adminEmail = $"admin@{slug}.school",
                adminPassword = "Admin123!ChangeMe"
            });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await create.Content.ReadFromJsonAsync<CreateTenantResponse>();
        body.Should().NotBeNull();
        body!.Slug.Should().Be(slug);
        body.DbName.Should().Be($"ss_t_{slug}");

        using var scope = Factory.Services.CreateScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var exists = await platformDb.Tenants.AnyAsync(t => t.Slug == slug);
        exists.Should().BeTrue();
    }

    private sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        Guid UserId,
        string Email);

    private sealed record CreateTenantResponse(
        Guid TenantId,
        string Slug,
        string DbName,
        string DbServer,
        Guid? AdminUserId);
}
