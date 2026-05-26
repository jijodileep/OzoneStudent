using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Identity;

public sealed class UserInvitation : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public Guid InvitedByUserId { get; set; }

    public bool IsValid => AcceptedAt is null && ExpiresAt > DateTime.UtcNow;
}
