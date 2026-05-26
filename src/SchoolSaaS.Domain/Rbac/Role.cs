using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Rbac;

public sealed class Role : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }
}
