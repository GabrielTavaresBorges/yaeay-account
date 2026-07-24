using FluentAssertions;
using YaeaY.Account.Domain.Errors.Emails;
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
        result.Error.Should().Be(EmailErrors.Required);
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
        result.Error.Should().Be(EmailErrors.Required);
    }


    [Fact]
    public void Create_WhenEmailContainsWhiteSpaceOnly_ShouldFailure()
    {
        // Arrange

        string emailAddress = " ";

        // Act

        var result = Email.Create(emailAddress);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
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
        result.Error.Should().Be(EmailErrors.TooLong(emailAddress.Length, 254));
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
    public void Create_WhenEmailViolatesFormatRules_ShouldFailure(string emailAddress)
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
    public void Create_WhenEmailIsValid_ShouldSuccess()
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
    public void Create_WhenEmailHasLeadingOrTrailingSpaces_ShouldSuccess()
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

    [Fact]
    public void Create_WhenEmailHasUppercaseCharacters_ShouldReturnLowercaseEmail()
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
