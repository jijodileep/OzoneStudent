using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Rbac;

public sealed class UserRole : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Guid? AssignedBy { get; set; }

    public Role? Role { get; set; }
}
