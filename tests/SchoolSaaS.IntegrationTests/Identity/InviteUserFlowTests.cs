using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Identity;

public sealed class InviteUserFlowTests : IntegrationTestBase
{
    public InviteUserFlowTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task InviteUser_AcceptInvitation_AllowsLogin()
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

        const string inviteEmail = "teacher@demo.school";
        var invite = await client.PostAsJsonAsync(
            "/api/v1/users/invite",
            new
            {
                email = inviteEmail,
                roleName = "tenant_admin",
                firstName = "Demo",
                lastName = "Teacher"
            });

        invite.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteBody = await invite.Content.ReadFromJsonAsync<InviteUserResponse>();
        inviteBody!.InvitationToken.Should().NotBeNullOrWhiteSpace();

        var anonymous = Factory.CreateClient();
        anonymous.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var accept = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/accept-invitation",
            new { token = inviteBody.InvitationToken, password = "Teacher123!ChangeMe" });

        accept.StatusCode.Should().Be(HttpStatusCode.OK);

        var teacherLogin = await anonymous.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = inviteEmail, password = "Teacher123!ChangeMe" });

        teacherLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record LoginResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt,
        Guid UserId,
        string Email);

    private sealed record InviteUserResponse(
        Guid InvitationId,
        string Email,
        DateTime ExpiresAt,
        string? InvitationToken);
}
