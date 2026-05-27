using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.CreateStaff;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
    }
}
