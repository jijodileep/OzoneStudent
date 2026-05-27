using FluentAssertions;
using SchoolSaaS.Application.Institution;
using SchoolSaaS.Domain.Institution;

namespace SchoolSaaS.UnitTests.Institution;

public sealed class CustomFieldValueValidatorTests
{
    [Fact]
    public void ValidateValue_RequiredEmpty_Fails()
    {
        var definition = new CustomFieldDefinition
        {
            Label = "Department",
            DataType = CustomFieldDataType.Text,
            IsRequired = true
        };

        var result = CustomFieldValueValidator.ValidateValue(definition, null);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ValidateValue_InvalidSelectOption_Fails()
    {
        var definition = new CustomFieldDefinition
        {
            Label = "Shift",
            DataType = CustomFieldDataType.Select,
            OptionsJson = "[\"Morning\",\"Evening\"]"
        };

        var result = CustomFieldValueValidator.ValidateValue(definition, "Night");

        result.IsFailure.Should().BeTrue();
    }
}
