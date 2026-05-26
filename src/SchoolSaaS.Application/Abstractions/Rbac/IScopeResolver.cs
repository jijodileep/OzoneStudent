using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Abstractions.Rbac;

public interface IScopeResolver
{
    Task<UserAccessScope> ResolveAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

public sealed record UserAccessScope(
    ScopeType ScopeType,
    IReadOnlyList<Guid> ScopeIds);
