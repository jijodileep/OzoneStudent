using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.GetStaffById;

public sealed class GetStaffByIdQueryHandler(
    ITenantContext tenantContext,
    IStaffRepository staffRepository,
    CustomFieldValueService customFieldValueService) : IRequestHandler<GetStaffByIdQuery, Result<GetStaffByIdResult>>
{
    public async Task<Result<GetStaffByIdResult>> Handle(
        GetStaffByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<GetStaffByIdResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var staff = await staffRepository.GetByIdAsync(tenantId, request.StaffId, cancellationToken);
        if (staff is null)
        {
            return Result<GetStaffByIdResult>.NotFound($"Staff member '{request.StaffId}' was not found.");
        }

        var values = await customFieldValueService.GetValuesAsync(
            tenantId,
            CustomFieldEntityType.Staff,
            staff.Id,
            cancellationToken);

        return Result<GetStaffByIdResult>.Success(new GetStaffByIdResult(
            staff.Id,
            staff.EmployeeCode,
            staff.Designation,
            staff.Status.ToString(),
            staff.JoinDate,
            staff.UserId,
            values));
    }
}
