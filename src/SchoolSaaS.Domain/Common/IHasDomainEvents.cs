using SchoolSaaS.Shared.Abstractions;

namespace SchoolSaaS.Domain.Common;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
