using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.Infrastructure.Persistence.Configurations;

public sealed class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("classes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.GradeId, x.AcademicYearId, x.Name }).IsUnique();
        builder.HasOne(x => x.Grade)
            .WithMany(x => x.Classes)
            .HasForeignKey(x => x.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AcademicYear)
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ClassTeacher)
            .WithMany()
            .HasForeignKey(x => x.ClassTeacherId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Sections)
            .WithOne(x => x.Class)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
