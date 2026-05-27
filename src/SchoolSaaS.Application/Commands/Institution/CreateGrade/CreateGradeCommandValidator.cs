using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.CreateGrade;

public sealed class CreateGradeCommandValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
