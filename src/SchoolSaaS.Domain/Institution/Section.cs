using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class Section : BaseEntity
{
    public Guid ClassId { get; set; }

    public SchoolClass Class { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }
}
