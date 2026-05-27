using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.LinkStaffToUser;

public sealed class LinkStaffToUserCommandValidator : AbstractValidator<LinkStaffToUserCommand>
{
    public LinkStaffToUserCommandValidator()
    {
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
