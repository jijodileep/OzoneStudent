namespace SchoolSaaS.Shared.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
