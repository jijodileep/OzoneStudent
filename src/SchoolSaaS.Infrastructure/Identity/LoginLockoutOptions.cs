namespace SchoolSaaS.Infrastructure.Identity;

public sealed class LoginLockoutOptions
{
    public const string SectionName = "LoginLockout";

    public int MaxFailedAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
