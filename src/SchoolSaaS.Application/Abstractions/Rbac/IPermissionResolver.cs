namespace SchoolSaaS.Application.Abstractions.Rbac;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> GetPermissionsAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        string permission,
        CancellationToken cancellationToken = default);

    Task InvalidateUserPermissionsCacheAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
