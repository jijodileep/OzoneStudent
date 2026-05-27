using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.CreateClass;

[RequirePermission(PermissionCodes.InstitutionClassesManage)]
public sealed record CreateClassCommand(
    Guid GradeId,
    Guid AcademicYearId,
    string Name,
    int Capacity,
    Guid? ClassTeacherId = null) : ICommand<CreateClassResult>;

public sealed record CreateClassResult(
    Guid Id,
    Guid GradeId,
    Guid AcademicYearId,
    string Name,
    int Capacity,
    Guid? ClassTeacherId);
