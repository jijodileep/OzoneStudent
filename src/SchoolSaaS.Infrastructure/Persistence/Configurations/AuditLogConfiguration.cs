using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Audit;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.BeforeJson).HasColumnType("json");
        builder.Property(x => x.AfterJson).HasColumnType("json");
        builder.Property(x => x.MetadataJson).HasColumnType("json");
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.Source).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Outcome).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
