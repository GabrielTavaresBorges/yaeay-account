using FluentAssertions;
using YaeaY.Account.Domain.Errors.Emails;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.UnitTests.ValueObjects.Emails.EmailTests;

public class EmailCreateTests
{
    // IsFailure

    [Fact]
    public void Create_WhenEmailIsNull_ShouldFail_WithEmailErrorsRequired()
    {
        // Arrange

        string emailAddressInvalid = null!;

        // Act

        var result = Email.Create(emailAddressInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
    }

    [Fact]
    public void Create_WhenEmailIsEmpty_ShouldFail_WithEmailErrorsRequired()
    {
        // Arrange

        string emailAddressInvalid = string.Empty;

        // Act

        var result = Email.Create(emailAddressInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
    }

    [Fact]
    public void Create_WhenEmailContainsWhiteSpaceOnly_ShouldFail_WithEmailErrorsRequired()
    {
        // Arrange

        string emailAddressInvalid = " ";

        // Act

        var result = Email.Create(emailAddressInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
    }

    [Fact]
    public void Create_WhenEmailIsTooLong_ShouldFail_WithEmailErrorsTooLong()
    {
        // Arrange

        string emailAddressInvalid = new string('a', 255) + "@example.com";

        // Act

        var result = Email.Create(emailAddressInvalid);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.TooLong(emailAddressInvalid.Length, 254));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData(".example@domain.com")]
    [InlineData("example.@domain.com")]
    [InlineData("example..name@domain.com")]
    [InlineData("example@-domain.com")]
    [InlineData("example@domain-.com")]
    [InlineData("example@domain..com")]
    [InlineData("example @domain.com")]
    public void Create_WhenEmailViolatesFormatRules_ShouldFail_WithEmailErrorsInvalidFormat(string emailAddress)
    {
        // Arrange

        string invalidFormat = emailAddress;

        // Act

        var result = Email.Create(invalidFormat);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.InvalidFormat);
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
    public void Create_WhenEmailHasLeadingOrTrailingSpaces_ShouldSucceed_WithTrimmedEmail()
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
    public void Create_WhenEmailHasExactlyMaximumLength_ShouldSucceed()
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

    [Fact]
    public void Create_WhenEmailHasUppercaseCharacters_ShouldSucceed_WithLowercaseEmail()
    {
        // Arrange

        string emailAddress = "Example@Domain.COM";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.EmailAddress.Should().Be("example@domain.com");
    }
}
