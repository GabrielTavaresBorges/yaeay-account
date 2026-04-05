using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.EmailConfirmationSettingsTests;

public class EmailConfirmationSettingsCreateTests
{
    // IsFailure

    #region FromEmail

    [Fact]
    public void Create_WhenFromEmailIsNull_ShouldThrowDomainException()
    {
        // Arrange

        Email fromEmail = null!;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("FROM_EMAIL_NULL");
        exeption.Message.Should().Be("From email cannot be null.");
    }

    #endregion

    #region FromName

    [Fact]
    public void Create_WhenFromNameIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string fromName = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("FROM_NAME_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("From name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenFromNameIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var fromName = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("FROM_NAME_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("From name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenFromNameContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var fromName = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("FROM_NAME_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("From name cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenFromNameIsTooLong_ShouldThrowDomainException()
    {
        // Arrange

        var fromName = new string('A', 151);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("FROM_NAME_TOO_LONG");
        exeption.Message.Should().Be("From name cannot be longer than 150 characters.");
    }

    #endregion

    #region Subject

    [Fact]
    public void Create_WhenSubjectIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string subject = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("SUBJECT_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Subject cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenSubjectIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var subject = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("SUBJECT_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Subject cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenSubjectContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var subject = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("SUBJECT_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Subject cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenSubjectIsTooLong_ShouldThrowDomainException()
    {
        // Arrange

        var subject = new string('A', 201);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("SUBJECT_TOO_LONG");
        exeption.Message.Should().Be("Subject cannot be longer than 200 characters.");
    }

    #endregion

    #region Body

    [Fact]
    public void Create_WhenBodyIsNull_ShouldThrowDomainException()
    {
        // Arrange

        string body = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("BODY_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Body HTML cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenBodyIsEmpty_ShouldThrowDomainException()
    {
        // Arrange

        var body = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("BODY_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Body HTML cannot be null, empty or white space.");
    }

    [Fact]
    public void Create_WhenBodyContainsWhiteSpaceOnly_ShouldThrowDomainException()
    {
        // Arrange

        var body = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Identifier.Should().Be("BODY_NULL_EMPTY_WHITE_SPACE");
        exeption.Message.Should().Be("Body HTML cannot be null, empty or white space.");
    }

    #endregion

    // IsSuccess

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSuccess()
    {
        // Arrange

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        var emailConfirmationSetting = EmailConfirmationSetting.Create(
            fromEmail,
            fromName,
            subject,
            body);

        // Assert

        emailConfirmationSetting.Should().NotBeNull();
        emailConfirmationSetting.FromEmail.Should().Be(fromEmail);
        emailConfirmationSetting.FromName.Should().Be(fromName);
        emailConfirmationSetting.Subject.Should().Be(subject);
        emailConfirmationSetting.BodyHtml.Should().Be(body);
        emailConfirmationSetting.IsActive.Should().BeTrue();
    }
}
