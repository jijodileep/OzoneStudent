using FluentAssertions;
using SchoolSaaS.Application.Commands.Institution.CreateClass;

namespace SchoolSaaS.UnitTests.Institution;

public sealed class CreateClassCommandValidatorTests
{
    private readonly CreateClassCommandValidator _validator = new();

    [Fact]
    public void Validate_ZeroCapacity_Fails()
    {
        var command = new CreateClassCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "1-A",
            0);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateClassCommand.Capacity));
    }
}
