using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateAcademicYear;

[RequirePermission(PermissionCodes.InstitutionAcademicYearsManage)]
public sealed record CreateAcademicYearCommand(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool SetAsCurrent = false) : ICommand<CreateAcademicYearResult>;

public sealed record CreateAcademicYearResult(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    string Status);
