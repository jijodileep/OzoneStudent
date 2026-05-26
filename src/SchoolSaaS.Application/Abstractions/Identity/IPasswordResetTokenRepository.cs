using SchoolSaaS.Domain.Identity;

namespace SchoolSaaS.Application.Abstractions.Identity;

public interface IPasswordResetTokenRepository
{
    Task CreateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task MarkUsedAsync(Guid tokenId, CancellationToken cancellationToken = default);

    Task InvalidateActiveForUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
