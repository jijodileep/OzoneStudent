namespace SchoolSaaS.Application.Abstractions.Institution;

public sealed record AcademicYearListFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string SortDirection = "asc",
    string? Status = null,
    bool? IsCurrent = null);
