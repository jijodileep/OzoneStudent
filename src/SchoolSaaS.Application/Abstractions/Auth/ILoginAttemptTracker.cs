namespace SchoolSaaS.Application.Abstractions.Auth;

public interface ILoginAttemptTracker
{
    Task<bool> IsLockedOutAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task RecordFailedAttemptAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task ClearAttemptsAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default);
}
