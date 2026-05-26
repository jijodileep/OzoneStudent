using FluentAssertions;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace SchoolSaaS.UnitTests.Identity;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ContainsTenantAndUserClaims()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            SecretKey = "unit_test_secret_key_at_least_32_chars",
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 30
        }));

        var token = service.GenerateAccessToken(
            new JwtUserContext(userId, tenantId, "user@test.com", ["tenant_admin"], ["auth.profile.read"]));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "auth.profile.read");
    }
}
