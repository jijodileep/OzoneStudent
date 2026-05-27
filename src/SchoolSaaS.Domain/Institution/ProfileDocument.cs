using SchoolSaaS.Domain.Common;

namespace SchoolSaaS.Domain.Institution;

public sealed class ProfileDocument : BaseEntity
{
    public ProfileDocumentOwnerType OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public Guid UploadedByUserId { get; set; }
}
