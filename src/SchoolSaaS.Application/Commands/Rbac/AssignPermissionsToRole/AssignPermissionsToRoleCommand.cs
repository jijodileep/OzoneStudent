using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Rbac.AssignPermissionsToRole;

[RequirePermission(PermissionCodes.RolesPermissionsAssign)]
public sealed record AssignPermissionsToRoleCommand(
    Guid RoleId,
    IReadOnlyList<string> Codes) : ICommand<bool>;
