using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Platform.Outbox;

public sealed class ProcessedEvent : PlatformEntity
{
    public Guid EventId { get; set; }

    public DateTime ProcessedAt { get; set; }
}
