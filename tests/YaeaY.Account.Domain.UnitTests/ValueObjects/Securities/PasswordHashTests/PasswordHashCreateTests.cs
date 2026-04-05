using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.PasswordHashTests;

public class PasswordHashCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPasswordHashIsNull_ShouldFailure()
    {
        // Arrange

        string hashed = null!;

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password hash cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenTokenHashIsEmpty_ShouldFailure()
    {
        // Arrange

        string hashed = string.Empty;

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password hash cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenTokenHashContainsWhiteSpace_ShouldFailure()
    {
        // Arrange

        string hashed = " ";

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_HASH_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password hash cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordHashIsTooLong_ShouldFailure()
    {
        // Arrange

        string hashed = new string('a', 1025);

        // Act

        var result = PasswordHash.Create(hashed);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_HASH_TOO_LONG");
        result.Error.Message.Should().Be("Password hash is too long. Current length: 1025. Max: 1024.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPasswordHashIsValid_ShouldSuccess()
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
    public void Create_WhenPasswordHashContainsLeadingAndTrailingSpaces_ShouldTrimAndSuccess()
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
    public void Create_WhenPasswordHashHasExactlyMaxLength_ShouldSuccess()
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
