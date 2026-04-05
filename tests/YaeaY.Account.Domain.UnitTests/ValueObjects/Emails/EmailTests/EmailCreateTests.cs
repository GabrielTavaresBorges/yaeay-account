using FluentAssertions;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Emails.EmailTests;

public class EmailCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenEmailIsNull_ShouldFailure()
    {
        // Arrange

        string emailAddress = null!;

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("EMAIL_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Email is required. " +
                                         "Please provide an address in the format 'example@domain.com'.");
    }

    [Fact]
    public void Create_WhenEmailIsEmpty_ShouldFailure()
    {
        // Arrange

        string emailAddress = string.Empty;

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("EMAIL_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Email is required. " +
                                         "Please provide an address in the format 'example@domain.com'.");
    }


    [Fact]
    public void Create_WhenEmailContainsWhiteSpace_ShouldFailure()
    {
        // Arrange

        string emailAddress = " ";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("EMAIL_NULL_EMPTY_WHITE_SPACE");
        result.Error.Message.Should().Be("Email is required. " +
                                         "Please provide an address in the format 'example@domain.com'.");
    }

    [Fact]
    public void Create_WhenEmailIsTooLong_ShouldFailure()
    {
        // Arrange

        string emailAddress = new string('a', 255) + "@example.com";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("EMAIL_TOO_LONG");
        result.Error.Message.Should().Be(
            $"Email is too long. " +
            $"Current length: {emailAddress.Length} characters. " +
            $"Maximum allowed length: 254 characters.");
    }

    [Fact]
    public void Create_WhenEmailFormatIsInvalid_ShouldFailure()
    {
        // Arrange

        string emailAddress = "invalid-email";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Identifier.Should().Be("EMAIL_INVALID_FORMAT");
        result.Error.Message.Should().Be(
            $"Email format is invalid. " +
            $"Expected format: 'example@domain.com'. " +
            $"Received value: '{emailAddress}'.");
    }

    // IsSuccess

    [Fact]
    public void Create_WhenEmailIsValid_ShouldSucceed()
    {
        // Arrange

        string emailAddress = "example@domain.com";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.EmailAddress.Should().Be(emailAddress);
    }

    [Fact]
    public void Create_WhenEmailHasLeadingOrTrailingSpaces_ShouldSucceed()
    {
        // Arrange

        string emailAddress = "  example@domain.com  ";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.EmailAddress.Should().Be("example@domain.com");
    }

    [Fact]
    public void Create_WhenEmailHasExactlyMaxLength_ShouldSuccess()
    {
        // Arrange

        string prefix = "example@domain";
        string suffix = ".com";
        string middle = new string('a', 236);

        string email = prefix + middle + suffix; ;

        // Act

        var result = Email.Create(email);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
