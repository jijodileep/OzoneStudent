using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListStaff;

[RequirePermission(PermissionCodes.InstitutionStaffManage)]
public sealed record ListStaffQuery : IQuery<ListStaffResult>;

public sealed record ListStaffResult(IReadOnlyList<StaffDto> Items);

public sealed record StaffDto(
    Guid Id,
    string EmployeeCode,
    string Designation,
    string Status,
    DateOnly JoinDate,
    Guid? UserId);
