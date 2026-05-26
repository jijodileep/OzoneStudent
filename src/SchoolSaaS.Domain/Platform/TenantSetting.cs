using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Platform;

public sealed class TenantSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
}
