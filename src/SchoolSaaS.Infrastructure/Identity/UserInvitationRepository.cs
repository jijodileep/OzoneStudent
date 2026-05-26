using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class UserInvitationRepository(ApplicationDbContext db) : IUserInvitationRepository
{
    public async Task CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        db.UserInvitations.Add(invitation);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<UserInvitation?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        db.UserInvitations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                i => !i.IsDeleted && i.TokenHash == tokenHash && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public Task<bool> HasPendingInvitationAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        db.UserInvitations
            .IgnoreQueryFilters()
            .AnyAsync(
                i => !i.IsDeleted
                     && i.TenantId == tenantId
                     && i.Email == normalizedEmail
                     && i.AcceptedAt == null
                     && i.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public Task MarkAcceptedAsync(Guid invitationId, CancellationToken cancellationToken = default) =>
        db.UserInvitations
            .Where(i => i.Id == invitationId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(i => i.AcceptedAt, DateTime.UtcNow)
                    .SetProperty(i => i.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
}
