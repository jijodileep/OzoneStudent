using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Domain.Audit;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Audit;

public sealed class AuditLogRepository(ApplicationDbContext db) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        AuditLogListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 200 ? 50 : filter.PageSize;

        var query = db.AuditLogs
            .AsNoTracking()
            .Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(a => a.Category == filter.Category);
        }

        if (filter.FromUtc is not null)
        {
            query = query.Where(a => a.CreatedAt >= filter.FromUtc.Value);
        }

        if (filter.ToUtc is not null)
        {
            query = query.Where(a => a.CreatedAt <= filter.ToUtc.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
