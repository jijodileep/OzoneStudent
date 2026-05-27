using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.SetCurrentAcademicYear;

public sealed class SetCurrentAcademicYearCommandValidator : AbstractValidator<SetCurrentAcademicYearCommand>
{
    public SetCurrentAcademicYearCommandValidator()
    {
        RuleFor(x => x.AcademicYearId).NotEmpty();
    }
}
