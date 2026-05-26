using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Platform.Outbox;
using SchoolSaaS.Shared.Events;

namespace SchoolSaaS.Infrastructure.Persistence.Interceptors;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AppendOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AppendOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AppendOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var aggregates = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            var integrationEvents = aggregate.DomainEvents
                .OfType<IntegrationEventBase>()
                .ToList();

            foreach (var integrationEvent in integrationEvents)
            {
                context.Set<OutboxMessage>().Add(new OutboxMessage
                {
                    EventId = integrationEvent.EventId,
                    TenantId = integrationEvent.TenantId,
                    EventType = integrationEvent.EventType,
                    PayloadJson = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonOptions),
                    OccurredAt = integrationEvent.OccurredAt
                });
            }

            aggregate.ClearDomainEvents();
        }
    }
}
