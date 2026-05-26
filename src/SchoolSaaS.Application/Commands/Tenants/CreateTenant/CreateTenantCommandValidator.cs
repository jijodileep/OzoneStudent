using FluentValidation;

namespace SchoolSaaS.Application.Commands.Tenants.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only.");
        RuleFor(x => x.Plan).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DbPort).GreaterThan(0).When(x => x.DbPort.HasValue);
        RuleFor(x => x.DbName).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.DbName));
        RuleFor(x => x.DbServer).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.DbServer));
        RuleFor(x => x.DbUser).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.DbUser));
        RuleFor(x => x.AdminEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.AdminEmail));
        RuleFor(x => x.AdminPassword)
            .MinimumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.AdminPassword));
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.AdminEmail) == string.IsNullOrWhiteSpace(x.AdminPassword))
            .WithMessage("AdminEmail and AdminPassword must both be provided or both omitted.");
    }
}

