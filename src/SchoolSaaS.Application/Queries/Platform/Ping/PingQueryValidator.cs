using FluentValidation;

namespace SchoolSaaS.Application.Queries.Platform.Ping;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        // Placeholder validator to prove assembly scanning; PingQuery has no inputs.
        RuleFor(x => x).NotNull();
    }
}

