using MediatR;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Queries.Institution.DownloadProfileDocument;

public sealed class DownloadStaffDocumentQueryHandler(ProfileDocumentService documents)
    : IRequestHandler<DownloadStaffDocumentQuery, Result<ProfileDocumentDownloadResult>>
{
    public Task<Result<ProfileDocumentDownloadResult>> Handle(
        DownloadStaffDocumentQuery request,
        CancellationToken cancellationToken) =>
        documents.OpenDownloadAsync(
            ProfileDocumentOwnerType.Staff,
            request.StaffId,
            request.DocumentId,
            cancellationToken);
}

public sealed class DownloadStudentDocumentQueryHandler(ProfileDocumentService documents)
    : IRequestHandler<DownloadStudentDocumentQuery, Result<ProfileDocumentDownloadResult>>
{
    public Task<Result<ProfileDocumentDownloadResult>> Handle(
        DownloadStudentDocumentQuery request,
        CancellationToken cancellationToken) =>
        documents.OpenDownloadAsync(
            ProfileDocumentOwnerType.Student,
            request.StudentId,
            request.DocumentId,
            cancellationToken);
}
