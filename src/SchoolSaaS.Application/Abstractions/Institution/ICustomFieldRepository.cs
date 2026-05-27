using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Application.Abstractions.Institution;

public interface ICustomFieldRepository
{
    Task<IReadOnlyList<CustomFieldDefinition>> ListDefinitionsAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<CustomFieldDefinition?> GetDefinitionByIdAsync(
        Guid tenantId,
        Guid definitionId,
        CancellationToken cancellationToken = default);

    Task<bool> DefinitionKeyExistsAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        string fieldKey,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<CustomFieldDefinition> AddDefinitionAsync(
        CustomFieldDefinition definition,
        CancellationToken cancellationToken = default);

    Task UpdateDefinitionAsync(
        CustomFieldDefinition definition,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomFieldValue>> ListValuesForEntityAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    Task UpsertValuesAsync(
        Guid tenantId,
        CustomFieldEntityType entityType,
        Guid entityId,
        IReadOnlyList<(CustomFieldDefinition Definition, string? Value)> values,
        CancellationToken cancellationToken = default);
}

public interface IProfileDocumentRepository
{
    Task<IReadOnlyList<ProfileDocument>> ListAsync(
        Guid tenantId,
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<ProfileDocument?> GetByIdAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<ProfileDocument> AddAsync(
        ProfileDocument document,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        ProfileDocument document,
        CancellationToken cancellationToken = default);
}

public interface IProfileDocumentOwnerValidator
{
    Task<bool> OwnerExistsAsync(
        Guid tenantId,
        ProfileDocumentOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default);
}
