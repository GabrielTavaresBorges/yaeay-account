using FluentAssertions;
using YaeaY.Account.Domain.Errors.PasswordText;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Securities.PasswordTextTests;

public class PasswordTextCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenPasswordTextIsNull_ShouldFail_WithPasswordTextErrorsRequired()
    {
        // Arrange

        string password = null!;

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordTextIsEmpty_ShouldFail_WithPasswordTextErrorsRequired()
    {
        // Arrange

        string password = string.Empty;

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordTextContainsWhiteSpaceOnly_ShouldFail_WithPasswordTextErrorsRequired()
    {
        // Arrange

        string password = " ";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.Required);
    }

    [Fact]
    public void Create_WhenPasswordTextIsTooShort_ShouldFail_WithPasswordTextErrorsTooShort()
    {
        // Arrange

        string password = "Ab1@abc";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.TooShort(password.Length, 8));
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainUppercase_ShouldFail_WithPasswordTextErrorsMissingUppercase()
    {
        // Arrange

        string password = "abc123@x";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.MissingUppercase);
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainLowercase_ShouldFail_WithPasswordTextErrorsMissingLowercase()
    {
        // Arrange

        string password = "ABC123@X";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.MissingLowercase);
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainDigit_ShouldFail_WithPasswordTextErrorsMissingDigit()
    {
        // Arrange

        string password = "Abcdef@X";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.MissingDigit);
    }

    [Fact]
    public void Create_WhenPasswordTextDoesNotContainSpecialCharacter_ShouldFail_WithPasswordTextErrorsMissingSpecialCharacter()
    {
        // Arrange

        string password = "Abcdef12";

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.MissingSpecialCharacter);
    }

    [Fact]
    public void Create_WhenPasswordTextIsTooLong_ShouldFail_WithPasswordTextErrorsTooLong()
    {
        // Arrange

        string password = "Aa1@" + new string('b', 253);

        // Act

        var result = PasswordText.Create(password);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PasswordTextErrors.TooLong(password.Length, 256));
    }

    // IsSuccess

    [Fact]
    public void Create_WhenPasswordTextIsValid_ShouldSucceed()
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
    public void Create_WhenPasswordTextHasLeadingOrTrailingSpaces_ShouldSucceed_WithTrimmedPasswordText()
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
    public void Create_WhenPasswordTextHasExactlyMaximumLength_ShouldSucceed()
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
