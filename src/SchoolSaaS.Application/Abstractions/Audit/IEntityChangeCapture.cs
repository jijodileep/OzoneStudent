namespace SchoolSaaS.Application.Abstractions.Audit;

public interface IEntityChangeCapture
{
    IReadOnlyList<CapturedEntityChange> GetChanges();

    void Add(CapturedEntityChange change);

    void Clear();
}

public sealed record CapturedEntityChange(
    string Action,
    string Category,
    string EntityType,
    Guid EntityId,
    string? BeforeJson,
    string? AfterJson);
