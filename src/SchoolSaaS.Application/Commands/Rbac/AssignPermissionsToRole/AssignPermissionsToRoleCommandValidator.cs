using FluentValidation;

namespace SchoolSaaS.Application.Commands.Rbac.AssignPermissionsToRole;

public sealed class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Codes).NotNull();
    }
}
