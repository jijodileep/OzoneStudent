using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Identity;

public sealed class LoginEndpointTests : IntegrationTestBase
{
    public LoginEndpointTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithDemoTenant_ReturnsTokens()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = IdentityDataSeeder.DefaultAdminEmail, password = IdentityDataSeeder.DefaultAdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.UserId.Should().NotBeEmpty();
    }

    private sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        Guid UserId,
        string Email);
}
