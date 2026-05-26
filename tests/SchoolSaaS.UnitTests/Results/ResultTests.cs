using FluentAssertions;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.UnitTests.Results;

public class ResultTests
{
    [Fact]
    public void Result_Success_HasNoErrors()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_Failure_ContainsErrorCode()
    {
        var result = Result.Failure("validation.failed", "Invalid input.");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "validation.failed");
    }

    [Fact]
    public void Result_GenericSuccess_HasValue()
    {
        var result = Result<string>.Success("ok");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public void Result_NotFound_UsesNotFoundCode()
    {
        var result = Result.NotFound("Tenant missing.");

        result.Errors.Should().ContainSingle(e => e.Code == ErrorFactory.NotFoundCode);
    }

    [Fact]
    public void Result_Forbidden_UsesForbiddenCode()
    {
        var result = Result.Forbidden();

        result.Errors.Should().ContainSingle(e => e.Code == ErrorFactory.ForbiddenCode);
    }

    [Fact]
    public void Result_Conflict_UsesConflictCode()
    {
        var result = Result.Conflict();

        result.Errors.Should().ContainSingle(e => e.Code == ErrorFactory.ConflictCode);
    }
}
