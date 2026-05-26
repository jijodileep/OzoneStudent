using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Platform.Outbox;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.Error).HasMaxLength(2000);

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
