using FluentAssertions;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;
using YaeaY.Account.Domain.Errors.EmailConfirmationTemplates;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.UnitTests.Entities.AggregateRoots.EmailConfirmationTemplatesTests;

public class EmailConfirmationTemplateCreateTests
{
    // IsFailure

    #region FromEmail

    [Fact]
    public void Create_WhenFromEmailIsNull_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsFromEmailRequired()
    {
        // Arrange

        Email fromEmailInvalid = null!;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmailInvalid, fromName, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.FromEmailRequired);
    }

    #endregion

    #region FromName

    [Fact]
    public void Create_WhenFromNameIsNull_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsFromNameRequired()
    {
        // Arrange

        string fromNameInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameIsEmpty_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsFromNameRequired()
    {
        // Arrange

        var fromNameInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsFromNameRequired()
    {
        // Arrange

        var fromNameInvalid  = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.FromNameRequired);
    }

    [Fact]
    public void Create_WhenFromNameIsTooLong_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsFromNameTooLong()
    {
        // Arrange

        var fromNameInvalid = new string('A', 151);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var subject = "Confirm your email address.";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromNameInvalid, subject, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.FromNameTooLong);
    }

    #endregion

    #region Subject

    [Fact]
    public void Create_WhenSubjectIsNull_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsSubjectRequired()
    {
        // Arrange

        string subjectInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectIsEmpty_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsSubjectRequired()
    {
        // Arrange

        var subjectInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsSubjectRequired()
    {
        // Arrange

        var subjectInvalid = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.SubjectRequired);
    }

    [Fact]
    public void Create_WhenSubjectIsTooLong_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsSubjectTooLong()
    {
        // Arrange

        var subjectInvalid = new string('A', 201);

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var body = "Welcome! Confirm your email to activate your account.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subjectInvalid, body);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.SubjectTooLong);
    }

    #endregion

    #region Body

    [Fact]
    public void Create_WhenBodyIsNull_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsBodyHtmlRequired()
    {
        // Arrange

        string bodyInvalid = null!;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.BodyHtmlRequired);
    }

    [Fact]
    public void Create_WhenBodyIsEmpty_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsBodyHtmlRequired()
    {
        // Arrange

        var bodyInvalid = string.Empty;

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.BodyHtmlRequired);
    }

    [Fact]
    public void Create_WhenBodyContainsWhiteSpaceOnly_ShouldThrowDomainException_WithEmailConfirmationTemplateErrorsBodyHtmlRequired()
    {
        // Arrange

        var bodyInvalid = " ";

        var emailAddressTest = "example@domain.com";
        var emailResult = Email.Create(emailAddressTest);
        var fromEmail = emailResult.Value;

        var fromName = "Example Account";
        var subject = "Confirm your email address.";

        // Act

        Action act = () => EmailConfirmationTemplate.Create(fromEmail, fromName, subject, bodyInvalid);

        // Assert

        var exeption = act.Should().Throw<DomainException>().Which;
        exeption.Error.Should().Be(EmailConfirmationTemplateErrors.BodyHtmlRequired);
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

        var emailConfirmationTemplate = EmailConfirmationTemplate.Create(
            fromEmail,
            fromName,
            subject,
            body);

        // Assert

        emailConfirmationTemplate.Should().NotBeNull();
        emailConfirmationTemplate.FromEmail.Should().Be(fromEmail);
        emailConfirmationTemplate.FromName.Should().Be(fromName);
        emailConfirmationTemplate.Subject.Should().Be(subject);
        emailConfirmationTemplate.BodyHtml.Should().Be(body);
        emailConfirmationTemplate.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenTemplateIsInactive_ShouldPreserveInactiveState()
    {
        // Arrange

        var emailResult = Email.Create("example@domain.com");

        // Act

        var emailConfirmationTemplate = EmailConfirmationTemplate.Create(
            emailResult.Value,
            "Example Account",
            "Confirm your email address.",
            "Welcome! Confirm your email to activate your account.",
            isActive: false);

        // Assert

        emailConfirmationTemplate.IsActive.Should().BeFalse();
    }
}
