using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Platform;

public sealed class Tenant : PlatformEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    // Data DB location (supports tenants on different MySQL servers).
    public string DbServer { get; set; } = "localhost";

    public int DbPort { get; set; } = 3306;

    public string DbName { get; set; } = string.Empty;

    public string DbUser { get; set; } = string.Empty;

    public string DbPassword { get; set; } = string.Empty;

    public TenantStatus Status { get; set; } = TenantStatus.Pending;

    public string Plan { get; set; } = "free";

    public string? SettingsJson { get; set; }
}
