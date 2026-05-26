using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Application.Abstractions.MultiTenancy;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.MultiTenancy;

public static class MultiTenancyDependencyInjection
{
    public static IServiceCollection AddMultiTenancyDatabases(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var platformConnection = configuration.GetConnectionString("Platform")
            ?? throw new InvalidOperationException(
                "Connection string 'Platform' is not configured.");

        services.Configure<TenancyOptions>(configuration.GetSection(TenancyOptions.SectionName));
        services.AddMemoryCache();
        services.AddDataProtection();
        services.AddSingleton<ITenantDbCredentialProtector, DataProtectionTenantDbCredentialProtector>();
        services.AddScoped<ITenantDatabaseCredentialsProvider, TenantDatabaseCredentialsProvider>();

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseMySql(platformConnection, MySqlServerVersionProvider.Version)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ITenantConnectionStringResolver, TenantConnectionStringResolver>();
        services.AddScoped<ITenantDatabaseProvisioner, TenantDatabaseProvisioner>();
        services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

        services.AddScoped<ApplicationDbContext>(sp =>
        {
            var tenantContext = sp.GetRequiredService<ITenantContext>();
            if (tenantContext.TenantId is null)
            {
                throw new InvalidOperationException(
                    "Tenant database access requires a resolved tenant. " +
                    "Platform catalog operations must use PlatformDbContext.");
            }

            var factory = sp.GetRequiredService<ITenantDbContextFactory>();
            return factory.CreateAsync(tenantContext.TenantId.Value).GetAwaiter().GetResult();
        });

        return services;
    }
}
