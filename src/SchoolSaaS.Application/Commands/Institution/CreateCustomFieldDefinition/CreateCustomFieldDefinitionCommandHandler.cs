using System.Text.Json;
using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;

public sealed class CreateCustomFieldDefinitionCommandHandler(
    ITenantContext tenantContext,
    ICustomFieldRepository repository) : IRequestHandler<CreateCustomFieldDefinitionCommand, Result<CustomFieldDefinitionDto>>
{
    public async Task<Result<CustomFieldDefinitionDto>> Handle(
        CreateCustomFieldDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CustomFieldDefinitionDto>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var fieldKey = request.FieldKey.Trim().ToLowerInvariant();

        if (await repository.DefinitionKeyExistsAsync(tenantId, request.EntityType, fieldKey, cancellationToken: cancellationToken))
        {
            return Result<CustomFieldDefinitionDto>.Conflict($"Custom field '{fieldKey}' already exists.");
        }

        var definition = new CustomFieldDefinition
        {
            TenantId = tenantId,
            EntityType = request.EntityType,
            FieldKey = fieldKey,
            Label = request.Label.Trim(),
            DataType = request.DataType,
            OptionsJson = request.DataType == CustomFieldDataType.Select
                ? JsonSerializer.Serialize(request.Options ?? [])
                : null,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        await repository.AddDefinitionAsync(definition, cancellationToken);
        return Result<CustomFieldDefinitionDto>.Success(Map(definition));
    }

    internal static CustomFieldDefinitionDto Map(CustomFieldDefinition definition)
    {
        IReadOnlyList<string>? options = null;
        if (!string.IsNullOrWhiteSpace(definition.OptionsJson))
        {
            options = JsonSerializer.Deserialize<string[]>(definition.OptionsJson) ?? [];
        }

        return new CustomFieldDefinitionDto(
            definition.Id,
            definition.EntityType.ToString(),
            definition.FieldKey,
            definition.Label,
            definition.DataType.ToString(),
            options,
            definition.IsRequired,
            definition.SortOrder,
            definition.IsActive);
    }
}
