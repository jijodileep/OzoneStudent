using SchoolSaaS.Application.Common;
using SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.UpdateCustomFieldDefinition;

[RequirePermission(PermissionCodes.InstitutionStaffFieldsManage)]
public sealed record UpdateCustomFieldDefinitionCommand(
    Guid DefinitionId,
    string Label,
    bool IsRequired,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<string>? Options = null) : ICommand<CustomFieldDefinitionDto>;
