using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Rbac.AssignRoleToUser;

[RequirePermission(PermissionCodes.RolesUserRoleAssign)]
public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : ICommand<bool>;
