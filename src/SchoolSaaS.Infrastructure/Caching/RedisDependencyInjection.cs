using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SchoolSaaS.Application.Abstractions;
using StackExchange.Redis;

namespace SchoolSaaS.Infrastructure.Caching;

public static class RedisDependencyInjection
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? new RedisOptions();

        var useMemoryCache = configuration.GetValue<bool>("Testing:UseDistributedMemoryCache");

        if (useMemoryCache)
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = $"{redisOptions.InstancePrefix}:";
            });
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
