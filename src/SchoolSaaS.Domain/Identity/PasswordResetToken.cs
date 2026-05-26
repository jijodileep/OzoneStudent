using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Identity;

public sealed class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public bool IsValid => UsedAt is null && ExpiresAt > DateTime.UtcNow;
}
