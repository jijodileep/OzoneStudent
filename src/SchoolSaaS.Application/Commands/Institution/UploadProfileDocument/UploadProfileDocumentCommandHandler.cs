using MediatR;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;

public sealed class UploadStaffDocumentCommandHandler(ProfileDocumentService documents)
    : IRequestHandler<UploadStaffDocumentCommand, Result<ProfileDocumentDto>>
{
    public Task<Result<ProfileDocumentDto>> Handle(
        UploadStaffDocumentCommand request,
        CancellationToken cancellationToken) =>
        documents.UploadAsync(
            ProfileDocumentOwnerType.Staff,
            request.StaffId,
            request.DocumentType,
            request.FileName,
            request.MimeType,
            request.FileSizeBytes,
            request.Content,
            cancellationToken);
}

public sealed class UploadStudentDocumentCommandHandler(ProfileDocumentService documents)
    : IRequestHandler<UploadStudentDocumentCommand, Result<ProfileDocumentDto>>
{
    public Task<Result<ProfileDocumentDto>> Handle(
        UploadStudentDocumentCommand request,
        CancellationToken cancellationToken) =>
        documents.UploadAsync(
            ProfileDocumentOwnerType.Student,
            request.StudentId,
            request.DocumentType,
            request.FileName,
            request.MimeType,
            request.FileSizeBytes,
            request.Content,
            cancellationToken);
}
