using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Platform.Outbox;

public sealed class OutboxMessage : PlatformEntity
{
    public Guid EventId { get; set; }

    public Guid TenantId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? Error { get; set; }

    public int RetryCount { get; set; }
}
