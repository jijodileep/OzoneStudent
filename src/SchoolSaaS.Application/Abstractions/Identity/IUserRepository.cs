using SchoolSaaS.Domain.Identity;

namespace SchoolSaaS.Application.Abstractions.Identity;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfile?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddAsync(User user, UserProfile profile, CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpdatePasswordAsync(
        Guid tenantId,
        Guid userId,
        string passwordHash,
        Guid? updatedBy,
        CancellationToken cancellationToken = default);
}
