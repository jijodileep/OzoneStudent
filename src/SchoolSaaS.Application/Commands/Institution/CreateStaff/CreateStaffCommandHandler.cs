using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.CreateStaff;

public sealed class CreateStaffCommandHandler(
    ITenantContext tenantContext,
    IStaffRepository staffRepository,
    IInstitutionReferenceRepository referenceRepository,
    CustomFieldValueService customFieldValueService) : IRequestHandler<CreateStaffCommand, Result<CreateStaffResult>>
{
    public async Task<Result<CreateStaffResult>> Handle(
        CreateStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateStaffResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var employeeCode = request.EmployeeCode.Trim().ToUpperInvariant();

        if (await staffRepository.ExistsByEmployeeCodeAsync(tenantId, employeeCode, cancellationToken: cancellationToken))
        {
            return Result<CreateStaffResult>.Conflict($"Employee code '{employeeCode}' already exists.");
        }

        if (request.UserId is Guid userId)
        {
            if (!await referenceRepository.UserExistsAsync(tenantId, userId, cancellationToken))
            {
                return Result<CreateStaffResult>.NotFound($"User '{userId}' was not found.");
            }

            if (await staffRepository.IsUserLinkedAsync(tenantId, userId, cancellationToken: cancellationToken))
            {
                return Result<CreateStaffResult>.Conflict("User is already linked to a staff profile.");
            }
        }

        var staff = new Staff
        {
            TenantId = tenantId,
            EmployeeCode = employeeCode,
            Designation = request.Designation.Trim(),
            JoinDate = request.JoinDate,
            UserId = request.UserId,
            Status = StaffStatus.Active
        };

        await staffRepository.AddAsync(staff, cancellationToken);

        var customFieldResult = await customFieldValueService.SetValuesAsync(
            tenantId,
            CustomFieldEntityType.Staff,
            staff.Id,
            request.CustomFields,
            cancellationToken);

        if (customFieldResult.IsFailure)
        {
            return Result<CreateStaffResult>.Failure(customFieldResult.Errors);
        }

        return Result<CreateStaffResult>.Success(new CreateStaffResult(
            staff.Id,
            staff.EmployeeCode,
            staff.Designation,
            staff.Status.ToString(),
            staff.JoinDate,
            staff.UserId));
    }
}
