using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Application.Abstractions.Files;

namespace SchoolSaaS.Infrastructure.Files;

public static class FileStorageDependencyInjection
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IFileStorageSettings>(sp => sp.GetRequiredService<IFileStorageService>() as IFileStorageSettings
            ?? throw new InvalidOperationException("File storage service must implement IFileStorageSettings."));
        return services;
    }
}
