using FluentValidation;
using SchoolSaaS.Application.Platform.Queries;

namespace SchoolSaaS.Application.Platform.Validators;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        // Placeholder validator to prove assembly scanning; PingQuery has no inputs.
        RuleFor(x => x).NotNull();
    }
}
