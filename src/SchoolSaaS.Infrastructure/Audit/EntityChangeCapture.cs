using SchoolSaaS.Application.Abstractions.Audit;

namespace SchoolSaaS.Infrastructure.Audit;

public sealed class EntityChangeCapture : IEntityChangeCapture
{
    private readonly List<CapturedEntityChange> _changes = [];

    public IReadOnlyList<CapturedEntityChange> GetChanges() => _changes;

    public void Add(CapturedEntityChange change) => _changes.Add(change);

    public void Clear() => _changes.Clear();
}
