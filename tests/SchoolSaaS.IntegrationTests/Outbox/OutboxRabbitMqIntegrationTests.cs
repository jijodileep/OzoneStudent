using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using RabbitMQ.Client;
using SchoolSaaS.Domain.Platform;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Application.Abstractions.MultiTenancy;
using SchoolSaaS.Infrastructure.MultiTenancy;
using SchoolSaaS.Infrastructure.Persistence;
using SchoolSaaS.Infrastructure.Persistence.Platform;
using SchoolSaaS.Shared.Events;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.IntegrationTests.Outbox;

[Collection(OutboxRabbitMqCollection.Name)]
public sealed class OutboxRabbitMqIntegrationTests(OutboxRabbitMqFixture fixture)
{
    private const string ExchangeName = "schoolsaas.events";

    [Fact]
    public async Task Pending_outbox_message_is_published_to_rabbitmq_and_marked_processed()
    {
        const string marker = "outbox-rmq-integration-marker";
        var tenantId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var mysql = new MySqlConnectionStringBuilder(fixture.BaseConnectionString);

        using (var setupScope = fixture.Factory.Services.CreateScope())
        {
            var platformDb = setupScope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var provisioner = setupScope.ServiceProvider.GetRequiredService<ITenantDatabaseProvisioner>();
            var credentialProtector = setupScope.ServiceProvider.GetRequiredService<ITenantDbCredentialProtector>();

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Outbox Test Tenant",
                Slug = $"outbox-{tenantId:N}",
                DbServer = mysql.Server ?? "localhost",
                DbPort = (int)mysql.Port,
                DbName = $"ss_t_{tenantId:N}",
                DbUser = mysql.UserID,
                DbPassword = credentialProtector.Protect(mysql.Password),
                Plan = "free",
                Status = TenantStatus.Active
            };

            platformDb.Tenants.Add(tenant);
            await platformDb.SaveChangesAsync();
            await provisioner.ProvisionAsync(tenant);
        }

        var integrationEvent = new TestIntegrationEvent(tenantId, marker);
        var payloadJson = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await using var consumer = await CreateConsumerAsync(fixture, CancellationToken.None);

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<TenantContext>().TenantId = tenantId;
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                TenantId = tenantId,
                EventType = nameof(TestIntegrationEvent),
                PayloadJson = payloadJson,
                OccurredAt = occurredAt
            });

            await db.SaveChangesAsync();
        }

        BasicGetResult? received = null;
        for (var attempt = 0; attempt < 60 && received is null; attempt++)
        {
            received = await consumer.Channel.BasicGetAsync(
                consumer.QueueName,
                autoAck: true,
                CancellationToken.None);

            if (received is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        received.Should().NotBeNull("outbox publisher should deliver to bound queue within timeout");
        var body = Encoding.UTF8.GetString(received!.Body.ToArray());
        body.Should().Contain(marker);
        received.RoutingKey.Should().Be(nameof(TestIntegrationEvent));

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<TenantContext>().TenantId = tenantId;
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var row = await db.OutboxMessages
                .AsNoTracking()
                .IgnoreQueryFilters()
                .SingleAsync(m => m.EventId == eventId);

            row.ProcessedAt.Should().NotBeNull();
            row.Error.Should().BeNullOrEmpty();
        }
    }

    private static async Task<ConsumerResources> CreateConsumerAsync(
        OutboxRabbitMqFixture fixture,
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = fixture.RabbitMqHost,
            Port = fixture.RabbitMqAmqpPort,
            UserName = "guest",
            Password = "guest"
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var queueDeclare = await channel.QueueDeclareAsync(
            queue: $"outbox.test.{Guid.NewGuid():N}",
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null,
            cancellationToken: cancellationToken);

        var queueName = queueDeclare.QueueName;

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: ExchangeName,
            routingKey: nameof(TestIntegrationEvent),
            arguments: null,
            cancellationToken: cancellationToken);

        return new ConsumerResources(connection, channel, queueName);
    }

    private sealed class ConsumerResources(
        IConnection connection,
        IChannel channel,
        string queueName) : IAsyncDisposable
    {
        public IChannel Channel => channel;

        public string QueueName => queueName;

        public async ValueTask DisposeAsync()
        {
            await channel.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
