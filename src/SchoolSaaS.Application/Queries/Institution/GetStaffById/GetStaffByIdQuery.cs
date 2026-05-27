using SchoolSaaS.Application.Common;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.GetStaffById;

[RequirePermission(PermissionCodes.InstitutionStaffManage)]
public sealed record GetStaffByIdQuery(Guid StaffId) : IQuery<GetStaffByIdResult>;

public sealed record GetStaffByIdResult(
    Guid Id,
    string EmployeeCode,
    string Designation,
    string Status,
    DateOnly JoinDate,
    Guid? UserId,
    IReadOnlyList<CustomFieldValueDto> CustomFields);
