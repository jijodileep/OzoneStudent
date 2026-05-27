using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.UpdateStaff;

public sealed class UpdateStaffCommandHandler(
    ITenantContext tenantContext,
    IStaffRepository staffRepository,
    CustomFieldValueService customFieldValueService) : IRequestHandler<UpdateStaffCommand, Result<UpdateStaffResult>>
{
    public async Task<Result<UpdateStaffResult>> Handle(
        UpdateStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<UpdateStaffResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var staff = await staffRepository.GetByIdAsync(tenantId, request.StaffId, cancellationToken);
        if (staff is null)
        {
            return Result<UpdateStaffResult>.NotFound($"Staff member '{request.StaffId}' was not found.");
        }

        staff.Designation = request.Designation.Trim();
        staff.Status = request.Status;
        staff.JoinDate = request.JoinDate;
        await staffRepository.UpdateAsync(staff, cancellationToken);

        if (request.CustomFields is not null)
        {
            var customFieldResult = await customFieldValueService.SetValuesAsync(
                tenantId,
                CustomFieldEntityType.Staff,
                staff.Id,
                request.CustomFields,
                cancellationToken);

            if (customFieldResult.IsFailure)
            {
                return Result<UpdateStaffResult>.Failure(customFieldResult.Errors);
            }
        }

        var values = await customFieldValueService.GetValuesAsync(
            tenantId,
            CustomFieldEntityType.Staff,
            staff.Id,
            cancellationToken);

        return Result<UpdateStaffResult>.Success(new UpdateStaffResult(
            staff.Id,
            staff.EmployeeCode,
            staff.Designation,
            staff.Status.ToString(),
            staff.JoinDate,
            staff.UserId,
            values));
    }
}
