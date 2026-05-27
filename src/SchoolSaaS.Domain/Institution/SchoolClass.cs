using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class SchoolClass : BaseEntity
{
    public Guid GradeId { get; set; }

    public Grade Grade { get; set; } = null!;

    public Guid AcademicYearId { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public Guid? ClassTeacherId { get; set; }

    public Staff? ClassTeacher { get; set; }

    public ICollection<Section> Sections { get; set; } = [];
}
