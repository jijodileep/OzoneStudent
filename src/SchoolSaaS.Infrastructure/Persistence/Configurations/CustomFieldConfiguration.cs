using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("custom_field_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldKey).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.OptionsJson).HasColumnType("json");
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.FieldKey }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.SortOrder });
    }
}

public sealed class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.ToTable("custom_field_values");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Value).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.DefinitionId }).IsUnique();
        builder.HasOne(x => x.Definition)
            .WithMany()
            .HasForeignKey(x => x.DefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProfileDocumentConfiguration : IEntityTypeConfiguration<ProfileDocument>
{
    public void Configure(EntityTypeBuilder<ProfileDocument> builder)
    {
        builder.ToTable("profile_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OwnerType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });
    }
}
