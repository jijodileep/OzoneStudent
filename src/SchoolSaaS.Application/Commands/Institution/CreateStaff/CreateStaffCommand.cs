using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateStaff;

[RequirePermission(PermissionCodes.InstitutionStaffManage)]
public sealed record CreateStaffCommand(
    string EmployeeCode,
    string Designation,
    DateOnly JoinDate,
    Guid? UserId = null,
    IReadOnlyDictionary<string, string?>? CustomFields = null) : ICommand<CreateStaffResult>;

public sealed record CreateStaffResult(
    Guid Id,
    string EmployeeCode,
    string Designation,
    string Status,
    DateOnly JoinDate,
    Guid? UserId);
