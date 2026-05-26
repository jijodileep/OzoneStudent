using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Audit.ListAuditLogs;

[RequirePermission(PermissionCodes.AuditLogsRead)]
public sealed record ListAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Action = null,
    string? Category = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IQuery<ListAuditLogsResult>;

public sealed record ListAuditLogsResult(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Action,
    string Category,
    string? EntityType,
    Guid? EntityId,
    string? Description,
    string Outcome,
    string Source,
    DateTime CreatedAt);

