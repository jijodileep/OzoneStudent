using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions;
using SchoolSaaS.Shared.MultiTenancy;
using StackExchange.Redis;

namespace SchoolSaaS.Infrastructure.Caching;

public sealed class RedisCacheService(
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    ITenantContext tenantContext,
    IOptions<RedisOptions> redisOptions) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await distributedCache.GetAsync(BuildKey(key), cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        var options = new DistributedCacheEntryOptions();

        if (expiration is not null)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        await distributedCache.SetAsync(BuildKey(key), bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        distributedCache.RemoveAsync(BuildKey(key), cancellationToken);

    public async Task RemoveByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenantId();
        var pattern = RedisCacheKeys.BuildTenantPattern(
            redisOptions.Value.InstancePrefix,
            tenantId,
            keyPrefix);

        var endpoints = connectionMultiplexer.GetEndPoints();
        if (endpoints.Length == 0)
        {
            return;
        }

        var server = connectionMultiplexer.GetServer(endpoints[0]);
        await foreach (var redisKey in server.KeysAsync(pattern: pattern).WithCancellation(cancellationToken))
        {
            await connectionMultiplexer.GetDatabase().KeyDeleteAsync(redisKey);
        }
    }

    private string BuildKey(string key)
    {
        var tenantId = RequireTenantId();
        return RedisCacheKeys.Build(redisOptions.Value.InstancePrefix, tenantId, key);
    }

    private Guid RequireTenantId() =>
        tenantContext.TenantId
        ?? throw new InvalidOperationException("Tenant context is required for cache operations.");
}
