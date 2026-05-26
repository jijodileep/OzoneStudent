using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Rbac.GetUserPermissions;

[RequirePermission(PermissionCodes.RolesRoleRead)]
[AuditRead(AuditCategories.Rbac)]
public sealed record GetUserPermissionsQuery(Guid? UserId = null) : IQuery<GetUserPermissionsResult>;

public sealed record GetUserPermissionsResult(
    Guid UserId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    string ScopeType,
    IReadOnlyList<Guid> ScopeIds);
