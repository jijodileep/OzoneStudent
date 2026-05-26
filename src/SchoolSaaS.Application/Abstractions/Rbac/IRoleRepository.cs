using SchoolSaaS.Domain.Rbac;

namespace SchoolSaaS.Application.Abstractions.Rbac;

public interface IRoleRepository
{
    Task<Role?> FindByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Role>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);

    Task AssignRoleToUserAsync(
        Guid tenantId,
        Guid userId,
        Guid roleId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default);

    Task ReplaceRolePermissionsAsync(
        Guid tenantId,
        Guid roleId,
        IReadOnlyList<string> permissionCodes,
        Guid? grantedBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionCodesForRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetUserIdsForRoleAsync(
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken = default);
}
