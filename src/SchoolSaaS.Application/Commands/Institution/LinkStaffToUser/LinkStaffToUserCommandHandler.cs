using MediatR;
using SchoolSaaS.Application.Abstractions.Institution;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.LinkStaffToUser;

public sealed class LinkStaffToUserCommandHandler(
    ITenantContext tenantContext,
    IStaffRepository staffRepository,
    IInstitutionReferenceRepository referenceRepository) : IRequestHandler<LinkStaffToUserCommand, Result<LinkStaffToUserResult>>
{
    public async Task<Result<LinkStaffToUserResult>> Handle(
        LinkStaffToUserCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<LinkStaffToUserResult>.Failure("institution.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var staff = await staffRepository.GetByIdAsync(tenantId, request.StaffId, cancellationToken);

        if (staff is null)
        {
            return Result<LinkStaffToUserResult>.NotFound($"Staff member '{request.StaffId}' was not found.");
        }

        if (!await referenceRepository.UserExistsAsync(tenantId, request.UserId, cancellationToken))
        {
            return Result<LinkStaffToUserResult>.NotFound($"User '{request.UserId}' was not found.");
        }

        if (await staffRepository.IsUserLinkedAsync(tenantId, request.UserId, staff.Id, cancellationToken))
        {
            return Result<LinkStaffToUserResult>.Conflict("User is already linked to another staff profile.");
        }

        staff.UserId = request.UserId;
        await staffRepository.UpdateAsync(staff, cancellationToken);

        return Result<LinkStaffToUserResult>.Success(new LinkStaffToUserResult(staff.Id, request.UserId));
    }
}
