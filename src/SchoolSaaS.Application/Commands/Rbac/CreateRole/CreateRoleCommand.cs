using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Rbac.CreateRole;

[RequirePermission(PermissionCodes.RolesRoleCreate)]
public sealed record CreateRoleCommand(string Name, string? Description = null) : ICommand<CreateRoleResult>;

public sealed record CreateRoleResult(Guid RoleId, string Name);
