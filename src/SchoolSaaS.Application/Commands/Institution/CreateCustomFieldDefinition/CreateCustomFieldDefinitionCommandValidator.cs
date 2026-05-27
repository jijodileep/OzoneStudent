using FluentValidation;

namespace SchoolSaaS.Application.Commands.Institution.CreateCustomFieldDefinition;

public sealed class CreateCustomFieldDefinitionCommandValidator : AbstractValidator<CreateCustomFieldDefinitionCommand>
{
    public CreateCustomFieldDefinitionCommandValidator()
    {
        RuleFor(x => x.FieldKey)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-z][a-z0-9_]*$")
            .WithMessage("Field key must be lowercase snake_case.");
        RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Options)
            .NotEmpty()
            .When(x => x.DataType == Domain.Institution.CustomFieldDataType.Select);
    }
}
