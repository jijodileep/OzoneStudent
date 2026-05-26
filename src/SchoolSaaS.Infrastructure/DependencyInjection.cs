using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Infrastructure.Caching;
using SchoolSaaS.Infrastructure.Identity;
using SchoolSaaS.Infrastructure.Events;
using SchoolSaaS.Infrastructure.Messaging;
using SchoolSaaS.Application.Abstractions.Platform;
using SchoolSaaS.Infrastructure.MultiTenancy;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Platform;
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
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<OutboxPublisherOptions>(configuration.GetSection(OutboxPublisherOptions.SectionName));

        services.AddRedisCaching(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddScoped<IEventPublisher, OutboxEventPublisher>();

        services.AddScoped<TenantSaveChangesInterceptor>();
        services.AddScoped<OutboxSaveChangesInterceptor>();
        services.AddScoped<EntityChangeCaptureInterceptor>();

        services.AddMultiTenancyDatabases(configuration);
        services.AddScoped<ITenantOnboardingService, TenantOnboardingService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<OutboxPublisherBackgroundService>();

        return services;
    }
}
