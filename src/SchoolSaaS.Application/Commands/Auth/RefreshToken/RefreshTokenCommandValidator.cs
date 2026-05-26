using FluentValidation;

namespace SchoolSaaS.Application.Commands.Auth.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() =>
        RuleFor(x => x.RefreshToken).NotEmpty();
}

