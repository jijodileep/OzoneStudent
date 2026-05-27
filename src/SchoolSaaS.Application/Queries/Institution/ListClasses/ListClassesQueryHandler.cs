using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListClasses;

public sealed class ListClassesQueryHandler(
    ITenantContext tenantContext,
    IClassRepository classRepository) : IRequestHandler<ListClassesQuery, Result<ListClassesResult>>
{
    public async Task<Result<ListClassesResult>> Handle(
        ListClassesQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListClassesResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var classes = await classRepository.ListAsync(
            tenantContext.TenantId.Value,
            request.GradeId,
            request.AcademicYearId,
            cancellationToken);

        var items = classes
            .Select(c => new ClassDto(
                c.Id,
                c.GradeId,
                c.Grade?.Name ?? string.Empty,
                c.AcademicYearId,
                c.Name,
                c.Capacity,
                c.ClassTeacherId,
                c.Sections
                    .OrderBy(s => s.Name)
                    .Select(s => new SectionDto(s.Id, s.Name, s.Capacity))
                    .ToList()))
            .ToList();

        return Result<ListClassesResult>.Success(new ListClassesResult(items));
    }
}
