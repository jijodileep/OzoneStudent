using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class PasswordResetTokenRepository(ApplicationDbContext db) : IPasswordResetTokenRepository
{
    public async Task CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<PasswordResetToken?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        db.PasswordResetTokens
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => !t.IsDeleted && t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public Task MarkUsedAsync(Guid tokenId, CancellationToken cancellationToken = default) =>
        db.PasswordResetTokens
            .Where(t => t.Id == tokenId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(t => t.UsedAt, DateTime.UtcNow)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

    public Task InvalidateActiveForUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.PasswordResetTokens
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(t => t.UsedAt, DateTime.UtcNow)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
}
