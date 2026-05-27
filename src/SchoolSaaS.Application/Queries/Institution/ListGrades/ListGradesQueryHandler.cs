using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListGrades;

public sealed class ListGradesQueryHandler(
    ITenantContext tenantContext,
    IGradeRepository gradeRepository) : IRequestHandler<ListGradesQuery, Result<ListGradesResult>>
{
    public async Task<Result<ListGradesResult>> Handle(
        ListGradesQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListGradesResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var grades = await gradeRepository.ListAsync(tenantContext.TenantId.Value, cancellationToken);
        var items = grades
            .Select(g => new GradeDto(g.Id, g.Name, g.Code, g.SortOrder))
            .ToList();

        return Result<ListGradesResult>.Success(new ListGradesResult(items));
    }
}
