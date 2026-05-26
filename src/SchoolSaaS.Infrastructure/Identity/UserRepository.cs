using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class UserRepository(ApplicationDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => !u.IsDeleted && u.TenantId == tenantId && u.Email == normalizedEmail,
                cancellationToken);

    public Task<User?> GetByIdAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => !u.IsDeleted && u.TenantId == tenantId && u.Id == userId,
                cancellationToken);

    public Task<UserProfile?> GetProfileAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        db.UserProfiles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => !p.IsDeleted && p.TenantId == tenantId && p.UserId == userId,
                cancellationToken);

    public Task<bool> EmailExistsAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default) =>
        db.Users
            .IgnoreQueryFilters()
            .AnyAsync(
                u => !u.IsDeleted && u.TenantId == tenantId && u.Email == normalizedEmail,
                cancellationToken);

    public async Task AddAsync(User user, UserProfile profile, CancellationToken cancellationToken = default)
    {
        db.Users.Add(user);
        db.UserProfiles.Add(profile);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(u => u.LastLoginAt, DateTime.UtcNow),
                cancellationToken);

    public Task UpdatePasswordAsync(
        Guid tenantId,
        Guid userId,
        string passwordHash,
        Guid? updatedBy,
        CancellationToken cancellationToken = default) =>
        db.Users
            .IgnoreQueryFilters()
            .Where(u => !u.IsDeleted && u.TenantId == tenantId && u.Id == userId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(u => u.PasswordHash, passwordHash)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow)
                    .SetProperty(u => u.UpdatedBy, updatedBy),
                cancellationToken);
}
