using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Identity;

public sealed class RefreshTokenRotationTests : IntegrationTestBase
{
    public RefreshTokenRotationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Refresh_WithOldToken_FailsAfterRotation()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = IdentityDataSeeder.DefaultAdminEmail, password = IdentityDataSeeder.DefaultAdminPassword });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>();
        tokens!.RefreshToken.Should().NotBeNullOrWhiteSpace();
        var originalRefresh = tokens!.RefreshToken;

        var refresh = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = originalRefresh });

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var reuse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = originalRefresh });

        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        Guid UserId,
        string Email);
}
