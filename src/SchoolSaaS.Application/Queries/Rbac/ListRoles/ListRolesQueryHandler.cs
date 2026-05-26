using MediatR;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Rbac.ListRoles;

public sealed class ListRolesQueryHandler(
    ITenantContext tenantContext,
    IRoleRepository roleRepository) : IRequestHandler<ListRolesQuery, Result<ListRolesResult>>
{
    public async Task<Result<ListRolesResult>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<ListRolesResult>.Failure("rbac.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var roles = await roleRepository.ListAsync(tenantId, cancellationToken);

        var dtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await roleRepository.GetPermissionCodesForRoleAsync(
                tenantId,
                role.Id,
                cancellationToken);
            dtos.Add(new RoleDto(role.Id, role.Name, role.Description, role.IsSystem, permissions));
        }

        return Result<ListRolesResult>.Success(new ListRolesResult(dtos));
    }
}
