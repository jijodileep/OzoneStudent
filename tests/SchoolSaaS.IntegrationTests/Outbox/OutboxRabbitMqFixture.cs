using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolSaaS.Infrastructure.Persistence;
using Testcontainers.MySql;
using Testcontainers.RabbitMq;

namespace SchoolSaaS.IntegrationTests.Outbox;

/// <summary>
/// MySQL + RabbitMQ + API host with outbox publisher enabled (for RabbitMQ publish integration tests).
/// </summary>
public sealed class OutboxRabbitMqFixture : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.0").Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:3-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string RabbitMqHost => _rabbit.Hostname;

    public int RabbitMqAmqpPort => _rabbit.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_mysql.StartAsync(), _rabbit.StartAsync());

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    _mysql.GetConnectionString());
                builder.UseSetting(
                    "ConnectionStrings:Redis",
                    "127.0.0.1:6379,abortConnect=false");
                builder.UseSetting(
                    "Redis:ConnectionString",
                    "127.0.0.1:6379,abortConnect=false");
                builder.UseSetting("Outbox:Enabled", "true");
                builder.UseSetting("Outbox:PollIntervalSeconds", "1");
                builder.UseSetting("Outbox:BatchSize", "50");
                builder.UseSetting("Outbox:MaxRetries", "5");
                builder.UseSetting("RabbitMq:Host", RabbitMqHost);
                builder.UseSetting("RabbitMq:Port", RabbitMqAmqpPort.ToString());
                builder.UseSetting("RabbitMq:Username", "guest");
                builder.UseSetting("RabbitMq:Password", "guest");
                builder.UseSetting("RabbitMq:Exchange", "schoolsaas.events");
            });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _mysql.DisposeAsync();
        await _rabbit.DisposeAsync();
    }
}
