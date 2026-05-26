using MediatR;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Domain.Rbac;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Rbac.CreateRole;

public sealed class CreateRoleCommandHandler(
    ITenantContext tenantContext,
    IRoleRepository roleRepository) : IRequestHandler<CreateRoleCommand, Result<CreateRoleResult>>
{
    public async Task<Result<CreateRoleResult>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return Result<CreateRoleResult>.Failure("rbac.tenant_required", "Tenant context is required.");
        }

        var tenantId = tenantContext.TenantId.Value;
        var name = request.Name.Trim().ToLowerInvariant();

        if (DefaultSystemRoles.All.Any(r => string.Equals(r.Name, name, StringComparison.Ordinal)))
        {
            return Result<CreateRoleResult>.Conflict($"Role name '{name}' is reserved for a system role.");
        }

        var existing = await roleRepository.FindByNameAsync(tenantId, name, cancellationToken);
        if (existing is not null)
        {
            return Result<CreateRoleResult>.Conflict($"Role '{name}' already exists.");
        }

        var role = new Role
        {
            TenantId = tenantId,
            Name = name,
            Description = request.Description?.Trim(),
            IsSystem = false
        };

        await roleRepository.AddAsync(role, cancellationToken);

        return Result<CreateRoleResult>.Success(new CreateRoleResult(role.Id, role.Name));
    }
}
