using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Rbac;

/// <summary>
/// Global permission catalog (not tenant-scoped).
/// </summary>
[ExcludeFromTenantFilter]
public sealed class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; } = true;
}
