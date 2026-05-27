using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class CustomFieldValue : BaseEntity
{
    public Guid DefinitionId { get; set; }

    public CustomFieldDefinition Definition { get; set; } = null!;

    public CustomFieldEntityType EntityType { get; set; }

    public Guid EntityId { get; set; }

    public string? Value { get; set; }
}
