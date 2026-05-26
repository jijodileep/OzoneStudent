using SchoolSaaS.Domain.Rbac;

namespace SchoolSaaS.Application.Abstractions.Rbac;

public interface IRoleRepository
{
    Task<Role?> FindByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);

    Task AssignRoleToUserAsync(
        Guid tenantId,
        Guid userId,
        Guid roleId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default);
}
