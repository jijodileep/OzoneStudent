using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Identity;

public sealed class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public string? PreferencesJson { get; set; }
}
