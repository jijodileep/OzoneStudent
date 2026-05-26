namespace SchoolSaaS.Shared.Events;

/// <summary>
/// Integration events are persisted via the outbox on SaveChanges.
/// Handlers should raise events on aggregates using <see cref="SchoolSaaS.Domain.Common.AggregateRoot.AddDomainEvent"/>.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(IntegrationEventBase integrationEvent, CancellationToken cancellationToken = default);
}
