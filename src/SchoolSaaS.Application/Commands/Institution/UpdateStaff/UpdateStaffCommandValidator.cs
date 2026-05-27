using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.UpdateStaff;

public sealed class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
    }
}
