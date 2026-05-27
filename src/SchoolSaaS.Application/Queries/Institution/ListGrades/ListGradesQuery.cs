using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListGrades;

[RequirePermission(PermissionCodes.InstitutionClassesManage)]
public sealed record ListGradesQuery : IQuery<ListGradesResult>;

public sealed record ListGradesResult(IReadOnlyList<GradeDto> Items);

public sealed record GradeDto(Guid Id, string Name, string Code, int SortOrder);
