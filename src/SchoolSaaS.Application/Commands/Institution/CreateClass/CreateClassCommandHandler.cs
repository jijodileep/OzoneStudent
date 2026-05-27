using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateClass;

public sealed class CreateClassCommandHandler(
    ITenantContext tenantContext,
    IGradeRepository gradeRepository,
    IClassRepository classRepository,
    IStaffRepository staffRepository,
    IInstitutionReferenceRepository referenceRepository) : IRequestHandler<CreateClassCommand, Result<CreateClassResult>>
{
    public async Task<Result<CreateClassResult>> Handle(
        CreateClassCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateClassResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var name = request.Name.Trim();

        if (await gradeRepository.GetByIdAsync(tenantId, request.GradeId, cancellationToken) is null)
        {
            return Result<CreateClassResult>.NotFound($"Grade '{request.GradeId}' was not found.");
        }

        if (!await referenceRepository.AcademicYearExistsAsync(tenantId, request.AcademicYearId, cancellationToken))
        {
            return Result<CreateClassResult>.NotFound($"Academic year '{request.AcademicYearId}' was not found.");
        }

        if (await classRepository.ExistsByNameAsync(
                tenantId,
                request.GradeId,
                request.AcademicYearId,
                name,
                cancellationToken: cancellationToken))
        {
            return Result<CreateClassResult>.Conflict(
                $"Class '{name}' already exists for this grade and academic year.");
        }

        if (request.ClassTeacherId is Guid teacherId)
        {
            var teacher = await staffRepository.GetByIdAsync(tenantId, teacherId, cancellationToken);
            if (teacher is null)
            {
                return Result<CreateClassResult>.NotFound($"Staff member '{teacherId}' was not found.");
            }

            if (teacher.Status != StaffStatus.Active)
            {
                return Result<CreateClassResult>.Failure(
                    "institution.staff_inactive",
                    "Class teacher must be an active staff member.");
            }
        }

        var schoolClass = new SchoolClass
        {
            TenantId = tenantId,
            GradeId = request.GradeId,
            AcademicYearId = request.AcademicYearId,
            Name = name,
            Capacity = request.Capacity,
            ClassTeacherId = request.ClassTeacherId
        };

        await classRepository.AddAsync(schoolClass, cancellationToken);

        return Result<CreateClassResult>.Success(new CreateClassResult(
            schoolClass.Id,
            schoolClass.GradeId,
            schoolClass.AcademicYearId,
            schoolClass.Name,
            schoolClass.Capacity,
            schoolClass.ClassTeacherId));
    }
}
