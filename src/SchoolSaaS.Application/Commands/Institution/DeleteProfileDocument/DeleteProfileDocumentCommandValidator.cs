using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.DeleteProfileDocument;

public sealed class DeleteStaffDocumentCommandValidator : AbstractValidator<DeleteStaffDocumentCommand>
{
    public DeleteStaffDocumentCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();
    }
}

public sealed class DeleteStudentDocumentCommandValidator : AbstractValidator<DeleteStudentDocumentCommand>
{
    public DeleteStudentDocumentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();
    }
}
