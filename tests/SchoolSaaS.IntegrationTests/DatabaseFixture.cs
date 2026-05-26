using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using Testcontainers.MySql;

namespace SchoolSaaS.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.0")
        .WithDatabase("schoolsaas_platform")
        .WithUsername("root")
        .WithPassword("password")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string BaseConnectionString => _mysql.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        var platformConnection = TestMysqlConnection.Platform(BaseConnectionString);
        var provisioningConnection = TestMysqlConnection.Provisioning(BaseConnectionString);
        var mysql = TestMysqlConnection.Parse(BaseConnectionString);

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Testing:SkipDataSeed", "true");
                builder.UseSetting("Testing:ExposeResetTokens", "true");
                builder.UseSetting("Testing:ExposeInvitationTokens", "true");
                builder.UseSetting("Tenancy:AllowHeaderTenantSlug", "true");
                builder.UseSetting("Tenancy:AllowHeaderTenantId", "true");
                builder.UseSetting("Tenancy:DefaultDbServer", mysql.Server ?? "localhost");
                builder.UseSetting("Tenancy:DefaultDbPort", ((int)mysql.Port).ToString());
                builder.UseSetting("Tenancy:DefaultDbUser", mysql.UserID);
                builder.UseSetting("Tenancy:DefaultDbPassword", mysql.Password);
                builder.UseSetting("Outbox:Enabled", "false");
                builder.UseSetting("ConnectionStrings:Platform", platformConnection);
                builder.UseSetting("ConnectionStrings:Provisioning", provisioningConnection);
                builder.UseSetting(
                    "ConnectionStrings:Redis",
                    "127.0.0.1:6379,abortConnect=false");
                builder.UseSetting(
                    "Redis:ConnectionString",
                    "127.0.0.1:6379,abortConnect=false");
            });

        using var scope = Factory.Services.CreateScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await platformDb.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _mysql.DisposeAsync();
    }
}
