using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Identity;

public sealed class LoginLockoutTests : IntegrationTestBase
{
    public LoginLockoutTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task Login_AfterMaxFailedAttempts_ReturnsLocked()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        const string email = "lockout-test@demo.school";

        for (var i = 0; i < 5; i++)
        {
            var attempt = await client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new { email, password = "wrong-password" });

            attempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var locked = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password = "wrong-password" });

        locked.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await locked.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Errors.Should().Contain(e => e.Code == "auth.account_locked");
    }

    private sealed record ErrorResponse(ErrorItem[] Errors);

    private sealed record ErrorItem(string Code, string Message);
}
