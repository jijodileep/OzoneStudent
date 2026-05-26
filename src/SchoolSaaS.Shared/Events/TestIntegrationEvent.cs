namespace SchoolSaaS.Shared.Events;

public sealed record TestIntegrationEvent(Guid TenantId, string Message)
    : IntegrationEventBase(TenantId);
