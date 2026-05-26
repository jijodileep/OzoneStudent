using SchoolSaaS.Domain.Identity;

namespace SchoolSaaS.Application.Abstractions.Identity;

public interface IUserInvitationRepository
{
    Task CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);

    Task<UserInvitation?> GetValidByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingInvitationAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task MarkAcceptedAsync(Guid invitationId, CancellationToken cancellationToken = default);
}
