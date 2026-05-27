using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateGrade;

public sealed class CreateGradeCommandHandler(
    ITenantContext tenantContext,
    IGradeRepository gradeRepository) : IRequestHandler<CreateGradeCommand, Result<CreateGradeResult>>
{
    public async Task<Result<CreateGradeResult>> Handle(
        CreateGradeCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateGradeResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var code = request.Code.Trim().ToUpperInvariant();

        if (await gradeRepository.ExistsByCodeAsync(tenantId, code, cancellationToken: cancellationToken))
        {
            return Result<CreateGradeResult>.Conflict($"Grade code '{code}' already exists.");
        }

        var grade = new Grade
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Code = code,
            SortOrder = request.SortOrder
        };

        await gradeRepository.AddAsync(grade, cancellationToken);

        return Result<CreateGradeResult>.Success(new CreateGradeResult(
            grade.Id,
            grade.Name,
            grade.Code,
            grade.SortOrder));
    }
}
