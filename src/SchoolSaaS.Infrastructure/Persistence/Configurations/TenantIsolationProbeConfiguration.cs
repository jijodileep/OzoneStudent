using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Platform;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class TenantIsolationProbeConfiguration : IEntityTypeConfiguration<TenantIsolationProbe>
{
    public void Configure(EntityTypeBuilder<TenantIsolationProbe> builder)
    {
        builder.ToTable("tenant_isolation_probes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Label });
    }
}
