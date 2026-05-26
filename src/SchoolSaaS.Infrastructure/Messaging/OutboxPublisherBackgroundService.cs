using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Messaging;

public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IRabbitMqConnectionFactory connectionFactory,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IOptions<OutboxPublisherOptions> outboxOptions,
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!outboxOptions.Value.Enabled)
        {
            logger.LogInformation("Outbox publisher is disabled via configuration.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher cycle failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(outboxOptions.Value.PollIntervalSeconds),
                stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var batchSize = outboxOptions.Value.BatchSize;
        var maxRetries = outboxOptions.Value.MaxRetries;

        var pending = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < maxRetries)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var exchange = rabbitMqOptions.Value.Exchange;
        await channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message.PayloadJson);
                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    MessageId = message.EventId.ToString(),
                    Type = message.EventType,
                    Timestamp = new AmqpTimestamp(
                        new DateTimeOffset(message.OccurredAt).ToUnixTimeSeconds())
                };

                await channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: message.EventType,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                if (message.RetryCount >= maxRetries)
                {
                    message.Error = $"DEAD_LETTER: {ex.Message}";
                    logger.LogError(
                        ex,
                        "Outbox message {EventId} moved to dead-letter after {RetryCount} attempts",
                        message.EventId,
                        message.RetryCount);
                }
                else
                {
                    logger.LogWarning(
                        ex,
                        "Failed to publish outbox message {EventId} (attempt {RetryCount})",
                        message.EventId,
                        message.RetryCount);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
