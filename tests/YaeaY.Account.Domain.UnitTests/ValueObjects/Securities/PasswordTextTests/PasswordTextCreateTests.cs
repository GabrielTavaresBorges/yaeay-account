using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.PasswordTextTests;

public class PasswordTextCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPasswordTextIsNull_ShouldFailure()
    {
        // Arrange

        string password = null!;

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordTextIsEmpty_ShouldFailure()
    {
        // Arrange

        string password = string.Empty;

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordTextContainsWhiteSpaceOnly_ShouldFailure()
    {
        // Arrange

        string password = " ";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordTextIsTooShort_ShouldFailure()
    {
        // Arrange

        string password = "Ab1@abc";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_TOO_SHORT");
        result.Error.Message.Should().Be("Password must be at least 8 chars.");
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainUppercase_ShouldFailure()
    {
        // Arrange

        string password = "abc123@x";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_MISSING_UPPERCASE");
        result.Error.Message.Should().Be("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainLowercase_ShouldFailure()
    {
        // Arrange

        string password = "ABC123@X";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_MISSING_LOWERCASE");
        result.Error.Message.Should().Be("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainDigit_ShouldFailure()
    {
        // Arrange

        string password = "Abcdef@X";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_MISSING_DIGIT");
        result.Error.Message.Should().Be("Password must contain at least one number.");
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainSpecialCharacter_ShouldFailure()
    {
        // Arrange

        string password = "Abcdef12";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_MISSING_SPECIAL");
        result.Error.Message.Should().Be("Password must contain at least one special character.");
    }

    [Fact]
    public void Create_WhenPasswordTextIsTooLong_ShouldFailure()
    {
        // Arrange

        string password = "Aa1@" + new string('b', 253);

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PASSWORD_TOO_LONG");
        result.Error.Message.Should().Be("Password is too long. Maximum allowed length is 256 characters.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPasswordTextIsValid_ShouldSuccess()
    {
        // Arrange

        string password = "Abc123@x";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("Abc123@x");
    }

    [Fact]
    public void Create_WhenPasswordTextHasLeadingOrTrailingSpaces_ShouldSuccess()
    {
        // Arrange

        string password = " Abc123@x ";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("Abc123@x");
    }

    [Fact]
    public void Create_WhenPasswordTextHasExactlyMaxLength_ShouldSuccess()
    {
        // Arrange

        string password = "Aa1!" + new string('a', 252);

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
