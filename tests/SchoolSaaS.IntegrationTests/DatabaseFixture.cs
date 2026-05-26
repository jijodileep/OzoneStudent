using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Infrastructure.Persistence;
using Testcontainers.MySql;

namespace SchoolSaaS.IntegrationTests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.0").Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Outbox:Enabled", "false");
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    _mysql.GetConnectionString());
                builder.UseSetting(
                    "ConnectionStrings:Redis",
                    "127.0.0.1:6379,abortConnect=false");
                builder.UseSetting(
                    "Redis:ConnectionString",
                    "127.0.0.1:6379,abortConnect=false");
            });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _mysql.DisposeAsync();
    }
}
