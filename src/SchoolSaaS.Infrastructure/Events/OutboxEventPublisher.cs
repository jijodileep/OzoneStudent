using SchoolSaaS.Shared.Events;

namespace SchoolSaaS.Infrastructure.Events;

/// <summary>
/// Integration events are written to the outbox by <see cref="Persistence.Interceptors.OutboxSaveChangesInterceptor"/>.
/// Raise events on aggregates before SaveChanges instead of calling this publisher directly.
/// </summary>
public sealed class OutboxEventPublisher : IEventPublisher
{
    public Task PublishAsync(IntegrationEventBase integrationEvent, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Direct publish is not supported. Add the integration event to an aggregate via AddDomainEvent and call SaveChanges.");
}
