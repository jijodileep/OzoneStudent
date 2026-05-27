using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.UploadProfileDocument;

public sealed class UploadStaffDocumentCommandValidator : AbstractValidator<UploadStaffDocumentCommand>
{
    internal static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public UploadStaffDocumentCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MimeType).Must(m => AllowedMimeTypes.Contains(m))
            .WithMessage("File type is not allowed.");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
    }
}

public sealed class UploadStudentDocumentCommandValidator : AbstractValidator<UploadStudentDocumentCommand>
{
    public UploadStudentDocumentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MimeType).Must(UploadStaffDocumentCommandValidator.AllowedMimeTypes.Contains)
            .WithMessage("File type is not allowed.");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
    }
}
