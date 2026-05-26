namespace SchoolSaaS.Shared.Events;

public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEventBase
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
