using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Domain.Common;
using SchoolSaaS.Infrastructure.Audit;

namespace SchoolSaaS.Infrastructure.Persistence.Interceptors;

public sealed class EntityChangeCaptureInterceptor(IEntityChangeCapture changeCapture) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity))
        {
            if (entry.Entity is not BaseEntity entity)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changeCapture.Add(new CapturedEntityChange(
                        AuditActions.Create,
                        ResolveCategory(entity),
                        entity.GetType().Name,
                        entity.Id,
                        BeforeJson: null,
                        AfterJson: Serialize(entity)));
                    break;

                case EntityState.Modified:
                    var before = entry.OriginalValues.Properties
                        .ToDictionary(p => p.Name, p => entry.OriginalValues[p.Name]);
                    changeCapture.Add(new CapturedEntityChange(
                        AuditActions.Update,
                        ResolveCategory(entity),
                        entity.GetType().Name,
                        entity.Id,
                        BeforeJson: JsonSerializer.Serialize(before, JsonOptions),
                        AfterJson: Serialize(entity)));
                    break;

                case EntityState.Deleted:
                    changeCapture.Add(new CapturedEntityChange(
                        AuditActions.Delete,
                        ResolveCategory(entity),
                        entity.GetType().Name,
                        entity.Id,
                        BeforeJson: Serialize(entity),
                        AfterJson: null));
                    break;
            }
        }
    }

    private static string Serialize(BaseEntity entity) =>
        JsonSerializer.Serialize(entity, entity.GetType(), JsonOptions);

    private static string ResolveCategory(BaseEntity entity) =>
        entity switch
        {
            Domain.Identity.User => AuditCategories.Auth,
            Domain.Rbac.Role or Domain.Rbac.RolePermission or Domain.Rbac.UserRole => AuditCategories.Rbac,
            Domain.Institution.AcademicYear or Domain.Institution.Term => AuditCategories.Institution,
            _ => AuditCategories.Platform
        };
}
