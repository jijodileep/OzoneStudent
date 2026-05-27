using System.Text.Json;
using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.UpdateCustomFieldDefinition;

public sealed class UpdateCustomFieldDefinitionCommandHandler(
    ITenantContext tenantContext,
    ICustomFieldRepository repository) : IRequestHandler<UpdateCustomFieldDefinitionCommand, Result<CustomFieldDefinitionDto>>
{
    public async Task<Result<CustomFieldDefinitionDto>> Handle(
        UpdateCustomFieldDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CustomFieldDefinitionDto>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var definition = await repository.GetDefinitionByIdAsync(tenantId, request.DefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result<CustomFieldDefinitionDto>.NotFound($"Custom field '{request.DefinitionId}' was not found.");
        }

        definition.Label = request.Label.Trim();
        definition.IsRequired = request.IsRequired;
        definition.SortOrder = request.SortOrder;
        definition.IsActive = request.IsActive;

        if (definition.DataType == CustomFieldDataType.Select)
        {
            definition.OptionsJson = JsonSerializer.Serialize(request.Options ?? []);
        }

        await repository.UpdateDefinitionAsync(definition, cancellationToken);
        return Result<CustomFieldDefinitionDto>.Success(CreateCustomFieldDefinitionCommandHandler.Map(definition));
    }
}
