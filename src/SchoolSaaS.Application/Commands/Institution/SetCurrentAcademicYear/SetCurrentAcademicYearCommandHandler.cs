using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.SetCurrentAcademicYear;

public sealed class SetCurrentAcademicYearCommandHandler(
    ITenantContext tenantContext,
    IAcademicYearRepository repository) : IRequestHandler<SetCurrentAcademicYearCommand, Result<SetCurrentAcademicYearResult>>
{
    public async Task<Result<SetCurrentAcademicYearResult>> Handle(
        SetCurrentAcademicYearCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<SetCurrentAcademicYearResult>.Failure(
                "institution.tenant_required",
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var academicYear = await repository.GetByIdAsync(tenantId, request.AcademicYearId, cancellationToken);

        if (academicYear is null)
        {
            return Result<SetCurrentAcademicYearResult>.NotFound(
                $"Academic year '{request.AcademicYearId}' was not found.");
        }

        await repository.SetCurrentAsync(tenantId, request.AcademicYearId, cancellationToken);

        return Result<SetCurrentAcademicYearResult>.Success(new SetCurrentAcademicYearResult(
            academicYear.Id,
            academicYear.Name,
            true,
            Domain.Institution.AcademicYearStatus.Active.ToString()));
    }
}
