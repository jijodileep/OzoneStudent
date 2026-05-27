using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.ListStaff;

public sealed class ListStaffQueryHandler(
    ITenantContext tenantContext,
    IStaffRepository staffRepository) : IRequestHandler<ListStaffQuery, Result<ListStaffResult>>
{
    public async Task<Result<ListStaffResult>> Handle(
        ListStaffQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListStaffResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var staffMembers = await staffRepository.ListAsync(tenantContext.TenantId.Value, cancellationToken);
        var items = staffMembers
            .Select(s => new StaffDto(
                s.Id,
                s.EmployeeCode,
                s.Designation,
                s.Status.ToString(),
                s.JoinDate,
                s.UserId))
            .ToList();

        return Result<ListStaffResult>.Success(new ListStaffResult(items));
    }
}
