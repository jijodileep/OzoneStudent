using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions.Auth;

namespace SchoolSaaS.Infrastructure.Identity;

public sealed class RedisLoginAttemptTracker(
    IDistributedCache cache,
    IOptions<LoginLockoutOptions> options) : ILoginAttemptTracker
{
    public async Task<bool> IsLockedOutAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var lockoutKey = BuildLockoutKey(tenantId, normalizedEmail);
        var lockout = await cache.GetStringAsync(lockoutKey, cancellationToken);
        return lockout is not null;
    }

    public async Task RecordFailedAttemptAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        var attemptsKey = BuildAttemptsKey(tenantId, normalizedEmail);
        var attempts = await cache.GetStringAsync(attemptsKey, cancellationToken);
        var count = int.TryParse(attempts, out var parsed) ? parsed + 1 : 1;

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(opts.LockoutMinutes)
        };

        await cache.SetStringAsync(
            attemptsKey,
            count.ToString(),
            cacheOptions,
            cancellationToken);

        if (count >= opts.MaxFailedAttempts)
        {
            await cache.SetStringAsync(
                BuildLockoutKey(tenantId, normalizedEmail),
                DateTime.UtcNow.ToString("O"),
                cacheOptions,
                cancellationToken);
        }
    }

    public async Task ClearAttemptsAsync(
        Guid tenantId,
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(BuildAttemptsKey(tenantId, normalizedEmail), cancellationToken);
        await cache.RemoveAsync(BuildLockoutKey(tenantId, normalizedEmail), cancellationToken);
    }

    private static string BuildAttemptsKey(Guid tenantId, string normalizedEmail) =>
        $"login-attempts:{tenantId:N}:{normalizedEmail}";

    private static string BuildLockoutKey(Guid tenantId, string normalizedEmail) =>
        $"login-lockout:{tenantId:N}:{normalizedEmail}";
}
