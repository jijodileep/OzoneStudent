using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateSection;

public sealed class CreateSectionCommandHandler(
    ITenantContext tenantContext,
    IClassRepository classRepository) : IRequestHandler<CreateSectionCommand, Result<CreateSectionResult>>
{
    public async Task<Result<CreateSectionResult>> Handle(
        CreateSectionCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateSectionResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var name = request.Name.Trim();

        var schoolClass = await classRepository.GetByIdAsync(tenantId, request.ClassId, cancellationToken);
        if (schoolClass is null)
        {
            return Result<CreateSectionResult>.NotFound($"Class '{request.ClassId}' was not found.");
        }

        if (await classRepository.SectionExistsByNameAsync(
                tenantId,
                request.ClassId,
                name,
                cancellationToken: cancellationToken))
        {
            return Result<CreateSectionResult>.Conflict($"Section '{name}' already exists in this class.");
        }

        var section = new Section
        {
            TenantId = tenantId,
            ClassId = request.ClassId,
            Name = name,
            Capacity = request.Capacity
        };

        await classRepository.AddSectionAsync(section, cancellationToken);

        return Result<CreateSectionResult>.Success(new CreateSectionResult(
            section.Id,
            section.ClassId,
            section.Name,
            section.Capacity));
    }
}
