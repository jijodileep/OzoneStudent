using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Identity;

public sealed class PasswordResetFlowTests : IntegrationTestBase
{
    public PasswordResetFlowTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task ForgotPassword_ResetPassword_AllowsLoginWithNewPassword()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = await CreateAuthenticatedClientAsync();
        const string resetUserEmail = "reset-user@demo.school";
        const string initialPassword = "ResetUser123!ChangeMe";

        var register = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                email = resetUserEmail,
                password = initialPassword,
                firstName = "Reset",
                lastName = "User",
                roleName = "tenant_admin"
            });

        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var anonymous = Factory.CreateClient();
        anonymous.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var forgot = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/forgot-password",
            new { email = resetUserEmail });

        forgot.StatusCode.Should().Be(HttpStatusCode.OK);
        var forgotBody = await forgot.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        forgotBody!.ResetToken.Should().NotBeNullOrWhiteSpace();

        var reset = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new { token = forgotBody.ResetToken, newPassword = "NewResetUser123!ChangeMe" });

        reset.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var loginOld = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = resetUserEmail, password = initialPassword });

        loginOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var loginNew = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = resetUserEmail, password = "NewResetUser123!ChangeMe" });

        loginNew.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_RequiresAuthentication()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync(
            "/api/v1/auth/me/password",
            new { currentPassword = IdentityDataSeeder.DefaultAdminPassword, newPassword = "AnotherAdmin123!ChangeMe" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = IdentityDataSeeder.DefaultAdminEmail, password = IdentityDataSeeder.DefaultAdminPassword });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        return client;
    }

    private sealed record ForgotPasswordResponse(string Message, string? ResetToken);

    private sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        Guid UserId,
        string Email);
}
