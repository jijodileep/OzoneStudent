using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Platform.Outbox;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
