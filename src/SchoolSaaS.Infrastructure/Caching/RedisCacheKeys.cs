namespace SchoolSaaS.Infrastructure.Caching;

public static class RedisCacheKeys
{
    public static string Build(string instancePrefix, Guid tenantId, string key) =>
        $"{instancePrefix}:tenant:{tenantId}:{key}";

    public static string BuildTenantPattern(string instancePrefix, Guid tenantId, string keyPrefix) =>
        $"{instancePrefix}:tenant:{tenantId}:{keyPrefix}*";
}
