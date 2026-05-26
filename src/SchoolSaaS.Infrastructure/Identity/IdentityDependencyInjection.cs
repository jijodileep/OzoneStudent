using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Application.Abstractions.Auth;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Identity;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Application.Abstractions.Notifications;
using SchoolSaaS.Infrastructure.Audit;
using SchoolSaaS.Infrastructure.Notifications;
using SchoolSaaS.Infrastructure.Rbac;

namespace SchoolSaaS.Infrastructure.Identity;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AppUrlOptions>(configuration.GetSection(AppUrlOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddSingleton<IEmailSender, LogEmailSender>();
        services.AddSingleton<IAppUrlProvider, AppUrlProvider>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGeneratorService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPermissionResolver, PermissionResolverService>();
        services.AddScoped<IAuditService, AuditLogService>();

        return services;
    }
}
