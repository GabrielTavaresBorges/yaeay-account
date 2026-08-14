using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;
using YaeaY.Account.Domain.Errors.EmailConfirmationSettings;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.EmailConfirmationSettingsTests;

public class EmailConfirmationSettingsCreateTests
{
    // IsFailure

    #region FromEmail

    [Fact]
    public void Create_WhenFromEmailIsNull_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsFromEmailRequired()
    {
        // Arrange

        Email fromEmailInvalid = null!;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmailInvalid, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.FromEmailRequired);
    }

    #endregion

    #region FromName

    [Fact]
    public void Create_WhenFromNameIsNull_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsFromNameRequired()
    {
        // Arrange

        string fromNameInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameIsEmpty_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsFromNameRequired()
    {
        // Arrange

        var fromNameInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsFromNameRequired()
    {
        // Arrange

        var fromNameInvalid  = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameIsTooLong_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsFromNameTooLong()
    {
        // Arrange

        var fromNameInvalid = new string('A', 151);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.FromNameTooLong);
    }

    #endregion

    #region Subject

    [Fact]
    public void Create_WhenSubjectIsNull_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsSubjectRequired()
    {
        // Arrange

        string subjectInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectIsEmpty_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsSubjectRequired()
    {
        // Arrange

        var subjectInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsSubjectRequired()
    {
        // Arrange

        var subjectInvalid = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectIsTooLong_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsSubjectTooLong()
    {
        // Arrange

        var subjectInvalid = new string('A', 201);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.SubjectTooLong);
    }

    #endregion

    #region Body

    [Fact]
    public void Create_WhenBodyIsNull_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsBodyHtmlRequired()
    {
        // Arrange

        string bodyInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.BodyHtmlRequired);
    }

    [Fact]
    public void Create_WhenBodyIsEmpty_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsBodyHtmlRequired()
    {
        // Arrange

        var bodyInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.BodyHtmlRequired);
    }

    [Fact]
    public void Create_WhenBodyContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationSettingErrorsBodyHtmlRequired()
    {
        // Arrange

        var bodyInvalid = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationSetting.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationSettingErrors.BodyHtmlRequired);
    }

    #endregion

    // IsSuccess

    [Fact]
    public void Create_WhenAllDataIsValid_ShouldSucceed()
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
