using SchoolSaaS.Application.Common;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;

[RequirePermission(PermissionCodes.InstitutionStaffFieldsManage)]
public sealed record CreateCustomFieldDefinitionCommand(
    CustomFieldEntityType EntityType,
    string FieldKey,
    string Label,
    CustomFieldDataType DataType,
    IReadOnlyList<string>? Options = null,
    bool IsRequired = false,
    int SortOrder = 0) : ICommand<CustomFieldDefinitionDto>;

public sealed record CustomFieldDefinitionDto(
    Guid Id,
    string EntityType,
    string FieldKey,
    string Label,
    string DataType,
    IReadOnlyList<string>? Options,
    bool IsRequired,
    int SortOrder,
    bool IsActive);
