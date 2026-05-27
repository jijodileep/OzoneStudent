using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.SetCurrentAcademicYear;

[RequirePermission(PermissionCodes.InstitutionAcademicYearsManage)]
public sealed record SetCurrentAcademicYearCommand(Guid AcademicYearId) : ICommand<SetCurrentAcademicYearResult>;

public sealed record SetCurrentAcademicYearResult(
    Guid Id,
    string Name,
    bool IsCurrent,
    string Status);
