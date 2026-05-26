using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.UnitTests.Identity;

public sealed class LoginAttemptTrackerTests
{
    [Fact]
    public async Task RecordFailedAttempts_AtThreshold_LocksOut()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var tracker = new RedisLoginAttemptTracker(
            cache,
            Options.Create(new LoginLockoutOptions { MaxFailedAttempts = 3, LockoutMinutes = 15 }));

        var tenantId = Guid.NewGuid();
        const string email = "user@test.com";

        await tracker.RecordFailedAttemptAsync(tenantId, email);
        await tracker.RecordFailedAttemptAsync(tenantId, email);
        (await tracker.IsLockedOutAsync(tenantId, email)).Should().BeFalse();

        await tracker.RecordFailedAttemptAsync(tenantId, email);
        (await tracker.IsLockedOutAsync(tenantId, email)).Should().BeTrue();
    }

    [Fact]
    public async Task ClearAttempts_RemovesLockout()
    {
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var tracker = new RedisLoginAttemptTracker(
            cache,
            Options.Create(new LoginLockoutOptions { MaxFailedAttempts = 1, LockoutMinutes = 15 }));

        var tenantId = Guid.NewGuid();
        const string email = "user@test.com";

        await tracker.RecordFailedAttemptAsync(tenantId, email);
        (await tracker.IsLockedOutAsync(tenantId, email)).Should().BeTrue();

        await tracker.ClearAttemptsAsync(tenantId, email);
        (await tracker.IsLockedOutAsync(tenantId, email)).Should().BeFalse();
    }
}
