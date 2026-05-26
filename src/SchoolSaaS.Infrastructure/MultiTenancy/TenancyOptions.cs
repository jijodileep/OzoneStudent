namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    /// <summary>Prefix for tenant DB names. Keep short — MySQL migration locks use the database name (64 char limit).</summary>
    public string DatabaseNamePrefix { get; set; } = "ss_t_";

    /// <summary>Fallback when a tenant row has no dedicated server (local dev only).</summary>
    public string DefaultDbServer { get; set; } = "localhost";

    public int DefaultDbPort { get; set; } = 3306;

    public string DefaultDbUser { get; set; } = "root";

    public string DefaultDbPassword { get; set; } = "password";

    /// <summary>Optional admin user for CREATE DATABASE on the tenant's server.</summary>
    public string? ProvisioningDbUser { get; set; }

    /// <summary>Optional admin password (plain in config, not stored in tenant table).</summary>
    public string? ProvisioningDbPassword { get; set; }
}
