using SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListCustomFieldDefinitions;

[RequirePermission(PermissionCodes.InstitutionStaffFieldsManage)]
public sealed record ListCustomFieldDefinitionsQuery(
    CustomFieldEntityType EntityType,
    bool ActiveOnly = true) : IQuery<ListCustomFieldDefinitionsResult>;

public sealed record ListCustomFieldDefinitionsResult(IReadOnlyList<CustomFieldDefinitionDto> Items);
