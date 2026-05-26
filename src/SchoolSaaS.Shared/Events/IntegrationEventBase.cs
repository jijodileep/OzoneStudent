using SchoolSaaS.Shared.Abstractions;

namespace SchoolSaaS.Shared.Events;

public abstract record IntegrationEventBase : IDomainEvent
{
    protected IntegrationEventBase(Guid tenantId)
    {
        TenantId = tenantId;
        EventType = GetType().Name;
        OccurredAt = DateTime.UtcNow;
    }

    public Guid EventId { get; init; } = Guid.NewGuid();

    public Guid TenantId { get; init; }

    public DateTime OccurredAt { get; init; }

    public string EventType { get; init; }
}
