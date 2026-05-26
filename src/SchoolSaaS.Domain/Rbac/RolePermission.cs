using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Rbac;

public sealed class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public Guid? GrantedBy { get; set; }

    public Role? Role { get; set; }

    public Permission? Permission { get; set; }
}
