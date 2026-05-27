using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class CustomFieldDefinition : BaseEntity
{
    public CustomFieldEntityType EntityType { get; set; }

    public string FieldKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public CustomFieldDataType DataType { get; set; }

    public string? OptionsJson { get; set; }

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
