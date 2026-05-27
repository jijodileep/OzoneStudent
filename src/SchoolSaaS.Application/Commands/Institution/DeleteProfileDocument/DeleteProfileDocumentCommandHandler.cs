using MediatR;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Commands.Institution.DeleteProfileDocument;

public sealed class DeleteStaffDocumentCommandHandler(ProfileDocumentService documents)
    : IRequestHandler<DeleteStaffDocumentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteStaffDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var result = await documents.DeleteAsync(
            ProfileDocumentOwnerType.Staff,
            request.StaffId,
            request.DocumentId,
            cancellationToken);

        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(result.Errors);
    }
}

public sealed class DeleteStudentDocumentCommandHandler(ProfileDocumentService documents)
    : IRequestHandler<DeleteStudentDocumentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteStudentDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var result = await documents.DeleteAsync(
            ProfileDocumentOwnerType.Student,
            request.StudentId,
            request.DocumentId,
            cancellationToken);

        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(result.Errors);
    }
}
