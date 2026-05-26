using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class RefreshTokenService(
    ApplicationDbContext db,
    IOptions<JwtOptions> jwtOptions) : IRefreshTokenService
{
    public async Task<RefreshTokenIssueResult> IssueAsync(
        Guid userId,
        Guid tenantId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var plainToken = SecureTokenGenerator.GeneratePlainToken();
        var hash = SecureTokenGenerator.HashToken(plainToken);
        var expiresAt = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenDays);

        db.RefreshTokens.Add(new RefreshToken
        {
            TenantId = tenantId,
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress
        });

        await db.SaveChangesAsync(cancellationToken);

        return new RefreshTokenIssueResult(plainToken, expiresAt);
    }

    public async Task<RefreshTokenValidationResult> ValidateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = SecureTokenGenerator.HashToken(refreshToken);
        var stored = await db.RefreshTokens
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return new RefreshTokenValidationResult(false, FailureReason: "Token not found.");
        }

        if (stored.RevokedAt is not null)
        {
            return new RefreshTokenValidationResult(false, FailureReason: "Token revoked.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            return new RefreshTokenValidationResult(false, FailureReason: "Token expired.");
        }

        return new RefreshTokenValidationResult(
            true,
            stored.UserId,
            stored.TenantId);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = SecureTokenGenerator.HashToken(refreshToken);
        var stored = await db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (stored is null || stored.RevokedAt is not null)
        {
            return;
        }

        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await db.RefreshTokens
            .IgnoreQueryFilters()
            .Where(t => t.UserId == userId && t.TenantId == tenantId && t.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow),
                cancellationToken);
    }

}
