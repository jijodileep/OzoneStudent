using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Audit.ListAuditLogs;

public sealed class ListAuditLogsQueryHandler(
    ITenantContext tenantContext,
    IAuditLogRepository auditLogRepository) : IRequestHandler<ListAuditLogsQuery, Result<ListAuditLogsResult>>
{
    public async Task<Result<ListAuditLogsResult>> Handle(
        ListAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListAuditLogsResult>.Failure(
                "audit.tenant_required",
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var filter = new AuditLogListFilter(
            request.Page,
            request.PageSize,
            request.Action,
            request.Category,
            request.FromUtc,
            request.ToUtc);

        var (items, total) = await auditLogRepository.ListAsync(tenantId, filter, cancellationToken);

        var dtos = items.Select(a => new AuditLogDto(
            a.Id,
            a.UserId,
            a.Action,
            a.Category,
            a.EntityType,
            a.EntityId,
            a.Description,
            a.Outcome,
            a.Source,
            a.CreatedAt)).ToList();

        return Result<ListAuditLogsResult>.Success(
            new ListAuditLogsResult(dtos, total, filter.Page, filter.PageSize));
    }
}

