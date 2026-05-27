using FluentValidation;
using SchoolSaaS.Shared.Errors;

namespace SchoolSaaS.Application.Commands.Institution.CreateAcademicYear;

public sealed class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(InstitutionErrorCodes.AcademicYear.NameRequired);

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithErrorCode(InstitutionErrorCodes.AcademicYear.NameTooLong);

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithErrorCode(InstitutionErrorCodes.AcademicYear.StartDateRequired);

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithErrorCode(InstitutionErrorCodes.AcademicYear.EndDateRequired);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithErrorCode(InstitutionErrorCodes.AcademicYear.EndDateBeforeStart);
    }
}
