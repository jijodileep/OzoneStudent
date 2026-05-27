using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListAcademicYears;

public sealed class ListAcademicYearsQueryHandler(
    ITenantContext tenantContext,
    IAcademicYearRepository repository) : IRequestHandler<ListAcademicYearsQuery, Result<ListAcademicYearsResult>>
{
    public async Task<Result<ListAcademicYearsResult>> Handle(
        ListAcademicYearsQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListAcademicYearsResult>.Failure(
                "institution.tenant_required",
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var filter = new AcademicYearListFilter(
            request.Page,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.SortDirection,
            request.Status,
            request.IsCurrent);

        var (years, totalCount) = await repository.ListPagedAsync(tenantId, filter, cancellationToken);

        var items = years
            .Select(y => new AcademicYearDto(
                y.Id,
                y.Name,
                y.StartDate,
                y.EndDate,
                y.IsCurrent,
                y.Status.ToString()))
            .ToList();

        return Result<ListAcademicYearsResult>.Success(
            new ListAcademicYearsResult(items, totalCount, filter.Page, filter.PageSize));
    }
}
