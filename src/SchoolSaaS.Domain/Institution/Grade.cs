using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class Grade : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<SchoolClass> Classes { get; set; } = [];
}
