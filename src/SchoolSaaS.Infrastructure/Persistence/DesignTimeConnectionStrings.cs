using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SchoolSaaS.Infrastructure.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Persistence;

internal static class DesignTimeConnectionStrings
{
    public static string ResolveTenantMigration(IConfiguration configuration)
    {
        var explicitConnection = configuration.GetConnectionString("TenantMigrations");
        if (!string.IsNullOrWhiteSpace(explicitConnection))
        {
            return explicitConnection;
        }

        var platformConnection = configuration.GetConnectionString("Platform");
        if (!string.IsNullOrWhiteSpace(platformConnection))
        {
            var prefix = configuration.GetSection(TenancyOptions.SectionName)["DatabaseNamePrefix"] ?? "ss_t_";
            var builder = new MySqlConnectionStringBuilder(platformConnection)
            {
                Database = $"{prefix}demo"
            };

            return builder.ConnectionString;
        }

        return "Server=localhost;Port=3306;Database=ss_t_demo;User=root;Password=password;";
    }
}
