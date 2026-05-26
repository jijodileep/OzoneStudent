using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class TermConfiguration : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> builder)
    {
        builder.ToTable("terms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.AcademicYearId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.AcademicYearId, x.SortOrder });
    }
}
