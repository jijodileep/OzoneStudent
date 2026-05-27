using FluentAssertions;
using SchoolSaaS.Application.Commands.Institution.CreateAcademicYear;

namespace SchoolSaaS.UnitTests.Institution;

public sealed class CreateAcademicYearCommandValidatorTests
{
    private readonly CreateAcademicYearCommandValidator _validator = new();

    [Fact]
    public void Validate_EndDateBeforeStartDate_Fails()
    {
        var command = new CreateAcademicYearCommand(
            "2026-27",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 7, 31));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateAcademicYearCommand.EndDate)
            && e.ErrorCode == "institution.academic_year.end_date_before_start");
    }

    [Fact]
    public void Validate_ValidDates_Succeeds()
    {
        var command = new CreateAcademicYearCommand(
            "2026-27",
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 3, 31));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
