using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class AcademicYear : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public AcademicYearStatus Status { get; set; } = AcademicYearStatus.Upcoming;

    public ICollection<Term> Terms { get; set; } = [];
}
