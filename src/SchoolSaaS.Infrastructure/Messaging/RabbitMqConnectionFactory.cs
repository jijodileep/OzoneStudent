using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace SchoolSaaS.Infrastructure.Messaging;

public sealed class RabbitMqConnectionFactory(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnectionFactory> logger) : IRabbitMqConnectionFactory
{
    public async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var rabbitOptions = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = rabbitOptions.Host,
            Port = rabbitOptions.Port,
            UserName = rabbitOptions.Username,
            Password = rabbitOptions.Password
        };

        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await factory.CreateConnectionAsync(cancellationToken);
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "RabbitMQ connection attempt {Attempt}/{MaxAttempts} failed",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
            }
        }

        return await factory.CreateConnectionAsync(cancellationToken);
    }
}
