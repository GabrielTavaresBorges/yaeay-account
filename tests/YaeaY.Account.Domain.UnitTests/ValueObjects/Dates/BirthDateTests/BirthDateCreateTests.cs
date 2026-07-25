using FluentAssertions;
using YaeaY.Account.Domain.Errors.BirthDate;
using YaeaY.Account.Domain.ValueObjects.Dates;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Dates.BirthDateTests;

public class BirthDateCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenBirthDateIsInFuture_ShouldFail_WithBirthDateErrorsInFuture()
    {
        // Arrange

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birthDate = today.AddDays(1);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BirthDateErrors.InFuture(birthDate, today));
    }

    [Fact]
    public void Create_WhenBirthDateExceedsMaximumAge_ShouldFail_WithBirthDateErrorsTooOld()
    {
        // Arrange

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minAllowed = today.AddYears(-150);
        var birthDate = minAllowed.AddDays(-1);

        // Act

        var result = BirthDate.Create(birthDate);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            BirthDateErrors.TooOld(birthDate, minAllowed, 150));
    }

    // IsSuccess

    [Fact]
    public void Create_WhenBirthDateIsValid_ShouldSucceed()
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
    public void Create_WhenBirthDateRepresentsExactlyMaximumAge_ShouldSucceed()
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
