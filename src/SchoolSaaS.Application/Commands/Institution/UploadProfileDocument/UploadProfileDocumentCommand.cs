using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;

public sealed record ProfileDocumentDto(
    Guid Id,
    string OwnerType,
    Guid OwnerId,
    string DocumentType,
    string FileName,
    string MimeType,
    long FileSizeBytes,
    DateTime UploadedAt);

[RequirePermission(PermissionCodes.InstitutionStaffDocumentsManage)]
public sealed record UploadStaffDocumentCommand(
    Guid StaffId,
    string DocumentType,
    string FileName,
    string MimeType,
    long FileSizeBytes,
    Stream Content) : ICommand<ProfileDocumentDto>;

[RequirePermission(PermissionCodes.StudentsDocumentsManage)]
public sealed record UploadStudentDocumentCommand(
    Guid StudentId,
    string DocumentType,
    string FileName,
    string MimeType,
    long FileSizeBytes,
    Stream Content) : ICommand<ProfileDocumentDto>;
