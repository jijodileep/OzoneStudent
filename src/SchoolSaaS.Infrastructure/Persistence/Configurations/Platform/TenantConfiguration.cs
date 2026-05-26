using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Platform;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations.Platform;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DbServer).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DbPort).IsRequired();
        builder.Property(x => x.DbName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DbUser).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DbPassword).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Plan).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SettingsJson).HasColumnType("json");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
