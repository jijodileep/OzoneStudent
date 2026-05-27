using FluentValidation;

namespace SchoolSaaS.Application.Queries.Institution.ListAcademicYears;

public sealed class ListAcademicYearsQueryValidator : AbstractValidator<ListAcademicYearsQuery>
{
    private static readonly string[] AllowedSortColumns =
    [
        "name",
        "startdate",
        "enddate",
        "status",
        "iscurrent"
    ];

    public ListAcademicYearsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortDirection)
            .Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase)
                       || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be asc or desc.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => sortBy is null
                            || AllowedSortColumns.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid sort column.");
    }
}
