using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListCustomFieldDefinitions;

public sealed class ListCustomFieldDefinitionsQueryHandler(
    ITenantContext tenantContext,
    ICustomFieldRepository repository) : IRequestHandler<ListCustomFieldDefinitionsQuery, Result<ListCustomFieldDefinitionsResult>>
{
    public async Task<Result<ListCustomFieldDefinitionsResult>> Handle(
        ListCustomFieldDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListCustomFieldDefinitionsResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var definitions = await repository.ListDefinitionsAsync(
            tenantContext.TenantId.Value,
            request.EntityType,
            request.ActiveOnly,
            cancellationToken);

        var items = definitions
            .Select(CreateCustomFieldDefinitionCommandHandler.Map)
            .ToList();

        return Result<ListCustomFieldDefinitionsResult>.Success(new ListCustomFieldDefinitionsResult(items));
    }
}
