using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Domain.Identity;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Queries.Auth.GetCurrentUser;

[RequirePermission(PermissionCodes.AuthProfileRead)]
[AuditRead(AuditCategories.Auth, nameof(User))]
public sealed record GetCurrentUserQuery : IQuery<CurrentUserDto>;

public sealed record CurrentUserDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    string ScopeType,
    IReadOnlyList<Guid> ScopeIds);

