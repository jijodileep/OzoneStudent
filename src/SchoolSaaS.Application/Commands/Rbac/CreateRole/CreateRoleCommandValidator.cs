using FluentValidation;

namespace SchoolSaaS.Application.Commands.Rbac.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z][a-z0-9_]*$")
            .WithMessage("Role name must be lowercase alphanumeric with underscores.");
    }
}
