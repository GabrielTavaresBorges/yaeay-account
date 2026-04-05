using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.PasswordPlainTextTests;

public class PasswordPlainTextCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPasswordPlainTextIsNull_ShouldFailure()
    {
        // Arrange

        string password = null!;

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextIsEmpty_ShouldFailure()
    {
        // Arrange

        string password = string.Empty;

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextContainsWhiteSpace_ShouldFailure()
    {
        // Arrange

        string password = " ";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Password cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextIsTooShort_ShouldFailure()
    {
        // Arrange

        string password = "Ab1@abc";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_TOO_SHORT");
        result.Error.Message.Should().Be("Password must be at least 8 chars.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextDoesNotContainUppercase_ShouldFailure()
    {
        // Arrange

        string password = "abc123@x";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_MISSING_UPPERCASE");
        result.Error.Message.Should().Be("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextDoesNotContainLowercase_ShouldFailure()
    {
        // Arrange

        string password = "ABC123@X";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_MISSING_LOWERCASE");
        result.Error.Message.Should().Be("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextDoesNotContainDigit_ShouldFailure()
    {
        // Arrange

        string password = "Abcdef@X";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_MISSING_DIGIT");
        result.Error.Message.Should().Be("Password must contain at least one number.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextDoesNotContainSpecialCharacter_ShouldFailure()
    {
        // Arrange

        string password = "Abcdef12";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_MISSING_SPECIAL");
        result.Error.Message.Should().Be("Password must contain at least one special character.");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextIsTooLong_ShouldFailure()
    {
        // Arrange

        string password = "Aa1@" + new string('b', 253);

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("PASSWORD_TOO_LONG");
        result.Error.Message.Should().Be("Password is too long. Maximum allowed length is 256 characters.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPasswordPlainTextIsValid_ShouldSuccess()
    {
        // Arrange

        string password = "Abc123@x";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("Abc123@x");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextHasLeadingOrTrailingSpaces_ShouldSucceed()
    {
        // Arrange

        string password = " Abc123@x ";

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Password.Should().Be("Abc123@x");
    }

    [Fact]
    public void Create_WhenPasswordPlainTextHasExactlyMaxLength_ShouldSuccess()
    {
        // Arrange

        string password = "Aa1!" + new string('a', 252);

        // Act

        var result = PasswordPlainText.Create(password);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
