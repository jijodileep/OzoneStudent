using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Rbac.ListRoles;

[RequirePermission(PermissionCodes.RolesRoleRead)]
public sealed record ListRolesQuery : IQuery<ListRolesResult>;

public sealed record ListRolesResult(IReadOnlyList<RoleDto> Items);

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    IReadOnlyList<string> Permissions);
