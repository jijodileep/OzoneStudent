using SchoolSaaS.Application.Common;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.UpdateStaff;

[RequirePermission(PermissionCodes.InstitutionStaffManage)]
public sealed record UpdateStaffCommand(
    Guid StaffId,
    string Designation,
    StaffStatus Status,
    DateOnly JoinDate,
    IReadOnlyDictionary<string, string?>? CustomFields = null) : ICommand<UpdateStaffResult>;

public sealed record UpdateStaffResult(
    Guid Id,
    string EmployeeCode,
    string Designation,
    string Status,
    DateOnly JoinDate,
    Guid? UserId,
    IReadOnlyList<CustomFieldValueDto> CustomFields);
