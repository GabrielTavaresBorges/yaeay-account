using FluentAssertions;
using YaeaY.Account.Domain.Errors.PasswordHash;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.PasswordHashTests;

public class PasswordHashCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPasswordHashIsNull_ShouldFail_WithPasswordHashErrorsRequired()
    {
        // Arrange

        string hashed = null!;

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordHashErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordHashIsEmpty_ShouldFail_WithPasswordHashErrorsRequired()
    {
        // Arrange

        string hashed = string.Empty;

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordHashErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordHashContainsWhiteSpaceOnly_ShouldFail_WithPasswordHashErrorsRequired()
    {
        // Arrange

        string hashed = " ";

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordHashErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordHashIsTooLong_ShouldFail_WithPasswordHashErrorsTooLong()
    {
        // Arrange

        string hashed = new string('a', 1025);

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordHashErrors.TooLong(hashed.Length, 1024));
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPasswordHashIsValid_ShouldSucceed()
    {
        // Arrange

        string hashed = "AQAAAAIAAYagAAAAEHashValido123==";

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("AQAAAAIAAYagAAAAEHashValido123==");
    }

    [Fact]
    public void Create_WhenPasswordHashContainsLeadingAndTrailingSpaces_ShouldSucceed_WithTrimmedPasswordHash()
    {
        // Arrange

        string hashed = "   AQAAAAIAAYagAAAAEHashValido123==   ";

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("AQAAAAIAAYagAAAAEHashValido123==");
    }

    [Fact]
    public void Create_WhenPasswordHashHasExactlyMaximumLength_ShouldSucceed()
    {
        // Arrange

        string hashed = new string('a', 1024);

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be(hashed);
    }
}
