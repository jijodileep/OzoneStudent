using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class Term : BaseEntity
{
    public Guid AcademicYearId { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int SortOrder { get; set; }
}
