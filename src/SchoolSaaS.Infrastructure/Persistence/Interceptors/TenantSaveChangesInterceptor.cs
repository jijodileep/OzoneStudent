using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Domain.Exceptions;
using SchoolSaaS.Shared.MultiTenancy;

namespace SchoolSaaS.Infrastructure.Persistence.Interceptors;

public sealed class TenantSaveChangesInterceptor(ITenantContextAccessor tenantContextAccessor)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var tenantContext = tenantContextAccessor.Current;
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantEntity tenantEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (tenantEntity.TenantId == Guid.Empty)
                        {
                            if (tenantContext.TenantId is null)
                            {
                                throw new InvalidOperationException(
                                    "Cannot add tenant-scoped entity without tenant context.");
                            }

                            tenantEntity.TenantId = tenantContext.TenantId.Value;
                        }
                        else if (tenantContext.TenantId is not null
                                 && tenantEntity.TenantId != tenantContext.TenantId)
                        {
                            throw new CrossTenantWriteException(
                                tenantEntity.TenantId,
                                tenantContext.TenantId);
                        }

                        if (entry.Entity is BaseEntity added)
                        {
                            added.CreatedAt = utcNow;
                            added.CreatedBy ??= tenantContext.UserId;
                        }

                        break;

                    case EntityState.Modified:
                        if (tenantContext.TenantId is not null
                            && tenantEntity.TenantId != tenantContext.TenantId)
                        {
                            throw new CrossTenantWriteException(
                                tenantEntity.TenantId,
                                tenantContext.TenantId);
                        }

                        if (entry.Entity is BaseEntity modified)
                        {
                            modified.UpdatedAt = utcNow;
                            modified.UpdatedBy = tenantContext.UserId;
                        }

                        break;
                }
            }
            else if (entry.Entity is PlatformEntity platformEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        platformEntity.CreatedAt = utcNow;
                        platformEntity.CreatedBy ??= tenantContext.UserId;
                        break;
                    case EntityState.Modified:
                        platformEntity.UpdatedAt = utcNow;
                        platformEntity.UpdatedBy = tenantContext.UserId;
                        break;
                }
            }
        }
    }
}
