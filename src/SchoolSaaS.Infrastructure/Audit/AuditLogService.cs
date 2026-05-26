using Microsoft.AspNetCore.Http;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Domain.Audit;
using SchoolSaaS.Infrastructure.MultiTenancy;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Audit;

public sealed class AuditLogService(
    ITenantDbContextFactory tenantDbFactory,
    ITenantContext tenantContext,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        if (tenantContext.TenantId is null)
        {
            return;
        }

        await using var db = await tenantDbFactory.CreateAsync(tenantContext.TenantId.Value, cancellationToken);
        var httpContext = httpContextAccessor.HttpContext;

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenantContext.TenantId,
            UserId = tenantContext.UserId,
            Action = entry.Action,
            Category = entry.Category,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Description = entry.Description,
            BeforeJson = entry.BeforeJson,
            AfterJson = entry.AfterJson,
            MetadataJson = entry.MetadataJson,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            CorrelationId = httpContext?.Items["CorrelationId"]?.ToString(),
            Source = entry.Source,
            Outcome = entry.Outcome,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
