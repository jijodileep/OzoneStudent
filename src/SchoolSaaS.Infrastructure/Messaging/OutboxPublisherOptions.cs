namespace SchoolSaaS.Infrastructure.Messaging;

public sealed class OutboxPublisherOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; set; } = true;

    public int PollIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 50;

    public int MaxRetries { get; set; } = 5;
}
