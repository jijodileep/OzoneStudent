using SchoolSaaS.Domain.Audit;

namespace SchoolSaaS.Application.Abstractions.Audit;

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        AuditLogListFilter filter,
        CancellationToken cancellationToken = default);
}

public sealed record AuditLogListFilter(
    int Page = 1,
    int PageSize = 50,
    string? Action = null,
    string? Category = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null);
