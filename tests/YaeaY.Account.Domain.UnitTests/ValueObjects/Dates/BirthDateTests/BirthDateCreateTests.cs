using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Dates;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Dates.BirthDateTests;

public class BirthDateCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenBirthDateIsInFuture_ShouldFailure()
    {
        // Arrange

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birthDate = today.AddDays(1);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BIRTH_DATE_IN_FUTURE");
        result.Error.Message.Should().Be(
            "Birth date cannot be in the future.\n" +
            $"Received: {birthDate:yyyy-MM-dd}.\n" +
            $"Today (UTC): {today:yyyy-MM-dd}.");
    }

    [Fact]
    public void Create_WhenBirthDateIsMoreThan150YearsAgo_ShouldFailure()
    {
        // Arrange

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minAllowed = today.AddYears(-150);
        var birthDate = minAllowed.AddDays(-1);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BIRTH_DATE_TOO_OLD");
        result.Error.Message.Should().Be(
            $"Birth date cannot be more than 150 years ago.\n" +
            $"Received: {birthDate:yyyy-MM-dd}.\n" +
            $"Minimum allowed (UTC): {minAllowed:yyyy-MM-dd}.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenBirthDateIsValid_ShouldSuccess()
    {
        // Arrange

        var birthDate = new DateOnly(2000, 1, 1);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Should().Be(birthDate);
    }

    [Fact]
    public void Create_WhenBirthDateIsExactly150YearsAgo_ShouldSuccess()
    {
        // Arrange

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birthDate = today.AddYears(-150);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Should().Be(birthDate);
    }
}
