using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Errors;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateAcademicYear;

public sealed class CreateAcademicYearCommandHandler(
    ITenantContext tenantContext,
    IAcademicYearRepository repository) : IRequestHandler<CreateAcademicYearCommand, Result<CreateAcademicYearResult>>
{
    public async Task<Result<CreateAcademicYearResult>> Handle(
        CreateAcademicYearCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateAcademicYearResult>.Failure(
                InstitutionErrorCodes.TenantRequired,
                "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var name = request.Name.Trim();

        if (await repository.ExistsByNameAsync(tenantId, name, cancellationToken: cancellationToken))
        {
            return Result<CreateAcademicYearResult>.Failure(
                InstitutionErrorCodes.AcademicYear.NameExists,
                $"Academic year '{name}' already exists.");
        }

        if (await repository.HasOverlappingDatesAsync(
                tenantId,
                request.StartDate,
                request.EndDate,
                cancellationToken: cancellationToken))
        {
            return Result<CreateAcademicYearResult>.Failure(
                InstitutionErrorCodes.AcademicYear.DatesOverlap,
                "Academic year dates overlap an existing academic year.");
        }

        var existingYears = await repository.ListAsync(tenantId, cancellationToken);
        var shouldBeCurrent = request.SetAsCurrent || existingYears.Count == 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var status = request.StartDate > today
            ? AcademicYearStatus.Upcoming
            : request.EndDate < today
                ? AcademicYearStatus.Closed
                : AcademicYearStatus.Active;

        var academicYear = new AcademicYear
        {
            TenantId = tenantId,
            Name = name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCurrent = shouldBeCurrent && existingYears.Count == 0,
            Status = status
        };

        await repository.AddAsync(academicYear, cancellationToken);

        if (shouldBeCurrent && existingYears.Count > 0)
        {
            await repository.SetCurrentAsync(tenantId, academicYear.Id, cancellationToken);
            academicYear.IsCurrent = true;
            academicYear.Status = AcademicYearStatus.Active;
        }

        return Result<CreateAcademicYearResult>.Success(new CreateAcademicYearResult(
            academicYear.Id,
            academicYear.Name,
            academicYear.StartDate,
            academicYear.EndDate,
            academicYear.IsCurrent,
            academicYear.Status.ToString()));
    }
}

