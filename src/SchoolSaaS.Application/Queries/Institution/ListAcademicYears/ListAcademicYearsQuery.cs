using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Institution.ListAcademicYears;

[RequirePermission(PermissionCodes.InstitutionAcademicYearsManage)]
public sealed record ListAcademicYearsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string SortDirection = "asc",
    string? Status = null,
    bool? IsCurrent = null) : IQuery<ListAcademicYearsResult>;

public sealed record ListAcademicYearsResult(
    IReadOnlyList<AcademicYearDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AcademicYearDto(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    string Status);
