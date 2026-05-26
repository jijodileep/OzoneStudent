using FluentAssertions;
using SchoolSaaS.Application.Commands.Auth.ForgotPassword;

namespace SchoolSaaS.UnitTests.Identity;

public sealed class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_email_empty()
    {
        var result = _validator.Validate(new ForgotPasswordCommand(string.Empty));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_pass_for_valid_email()
    {
        var result = _validator.Validate(new ForgotPasswordCommand("admin@demo.school"));
        result.IsValid.Should().BeTrue();
    }
}
