using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListClasses;

[RequirePermission(PermissionCodes.InstitutionClassesManage)]
public sealed record ListClassesQuery(
    Guid? GradeId = null,
    Guid? AcademicYearId = null) : IQuery<ListClassesResult>;

public sealed record ListClassesResult(IReadOnlyList<ClassDto> Items);

public sealed record ClassDto(
    Guid Id,
    Guid GradeId,
    string GradeName,
    Guid AcademicYearId,
    string Name,
    int Capacity,
    Guid? ClassTeacherId,
    IReadOnlyList<SectionDto> Sections);

public sealed record SectionDto(Guid Id, string Name, int Capacity);
