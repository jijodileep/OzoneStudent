using Microsoft.EntityFrameworkCore;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Infrastructure.Persistence;

namespace SchoolSaaS.Infrastructure.Institution;

public sealed class CustomFieldRepository(ApplicationDbContext db) : ICustomFieldRepository
{
    public async Task<IReadOnlyList<CustomFieldDefinition>> ListDefinitionsAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = db.CustomFieldDefinitions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);
    }

    public Task<CustomFieldDefinition?> GetDefinitionByIdAsync(
        Guid tenantId,
        Guid definitionId,
        CancellationToken cancellationToken = default) =>
        db.CustomFieldDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == definitionId,
            cancellationToken);

    public Task<bool> DefinitionKeyExistsAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        string fieldKey,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        db.CustomFieldDefinitions.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                     && x.EntityType == entityType
                     && x.FieldKey == fieldKey
                     && (excludeId == null || x.Id != excludeId),
                cancellationToken);

    public async Task<CustomFieldDefinition> AddDefinitionAsync(
        CustomFieldDefinition definition,
        CancellationToken cancellationToken = default)
    {
        db.CustomFieldDefinitions.Add(definition);
        await db.SaveChangesAsync(cancellationToken);
        return definition;
    }

    public async Task UpdateDefinitionAsync(
        CustomFieldDefinition definition,
        CancellationToken cancellationToken = default)
    {
        db.CustomFieldDefinitions.Update(definition);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomFieldValue>> ListValuesForEntityAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default) =>
        await db.CustomFieldValues.AsNoTracking()
            .Include(x => x.Definition)
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId)
            .ToListAsync(cancellationToken);

    public async Task UpsertValuesAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        IReadOnlyList<(CustomFieldDefinition Definition, string? Value)> values,
        CancellationToken cancellationToken = default)
    {
        var definitionIds = values.Select(v => v.Definition.Id).ToList();
        var existing = await db.CustomFieldValues
            .Where(x => x.TenantId == tenantId
                        && x.EntityType == entityType
                        && x.EntityId == entityId
                        && definitionIds.Contains(x.DefinitionId))
            .ToListAsync(cancellationToken);

        foreach (var (definition, value) in values)
        {
            var current = existing.FirstOrDefault(x => x.DefinitionId == definition.Id);
            if (current is null)
            {
                db.CustomFieldValues.Add(new CustomFieldValue
                {
                    TenantId = tenantId,
                    DefinitionId = definition.Id,
                    EntityType = entityType,
                    EntityId = entityId,
                    Value = value
                });
            }
            else
            {
                current.Value = value;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ProfileDocumentRepository(ApplicationDbContext db) : IProfileDocumentRepository
{
    public async Task<IReadOnlyList<ProfileDocument>> ListAsync(
        Guid tenantId,
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default) =>
        await db.ProfileDocuments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OwnerType == ownerType && x.OwnerId == ownerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ProfileDocument?> GetByIdAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken = default) =>
        db.ProfileDocuments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == documentId,
            cancellationToken);

    public async Task<ProfileDocument> AddAsync(
        ProfileDocument document,
        CancellationToken cancellationToken = default)
    {
        db.ProfileDocuments.Add(document);
        await db.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task SoftDeleteAsync(
        ProfileDocument document,
        CancellationToken cancellationToken = default)
    {
        document.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ProfileDocumentOwnerValidator(ApplicationDbContext db) : IProfileDocumentOwnerValidator
{
    public Task<bool> OwnerExistsAsync(
        Guid tenantId,
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default) =>
        ownerType switch
        {
            ProfileDocumentOwnerType.Staff => db.StaffMembers.AsNoTracking()
                .AnyAsync(x => x.TenantId == tenantId && x.Id == ownerId, cancellationToken),
            ProfileDocumentOwnerType.Student => Task.FromResult(false),
            _ => Task.FromResult(false)
        };
}
