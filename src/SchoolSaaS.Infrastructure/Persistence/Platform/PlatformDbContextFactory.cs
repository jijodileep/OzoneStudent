using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SchoolSaaS.Infrastructure.Persistence.Platform;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SchoolSaaS.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Development.local.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Platform")
            ?? "Server=localhost;Port=3306;Database=schoolsaas_platform;User=root;Password=password;";

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseMySql(connectionString, MySqlServerVersionProvider.Version)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PlatformDbContext(options);
    }
}
