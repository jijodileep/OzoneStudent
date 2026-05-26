using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Platform;

/// <summary>
/// Tenant-scoped entity used by integration tests to verify global query filters
/// and cross-tenant write protection. Not exposed via API.
/// </summary>
public sealed class TenantIsolationProbe : BaseEntity
{
    public string Label { get; set; } = string.Empty;
}
