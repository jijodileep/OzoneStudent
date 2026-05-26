using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Identity;

public sealed class User : BaseEntity, IAggregateRoot
{
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool EmailConfirmed { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
