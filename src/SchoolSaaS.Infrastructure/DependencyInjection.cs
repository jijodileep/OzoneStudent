using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Infrastructure.Caching;
using SchoolSaaS.Infrastructure.Events;
using SchoolSaaS.Infrastructure.Messaging;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Interceptors;
using SchoolSaaS.Shared.Events;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<OutboxPublisherOptions>(configuration.GetSection(OutboxPublisherOptions.SectionName));

        services.AddRedisCaching(configuration);

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddScoped<IEventPublisher, OutboxEventPublisher>();

        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<OutboxSaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseMySql(connectionString, MySqlServerVersionProvider.Version)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<TenantSaveChangesInterceptor>(),
                    sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<OutboxPublisherBackgroundService>();

        return services;
    }
}
