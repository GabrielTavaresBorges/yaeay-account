using FluentAssertions;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UnitTests.Services.Emails.EmailConfirmationMessageComposerTests;

public sealed class EmailConfirmationMessageComposerTests
{
    private const string ValidBodyHtml = """
        <p>Hello, {{FullName}}.</p>
        <a href="{{ConfirmationUrl}}">Confirm email</a>
        """;

    [Fact]
    public void Compose_WhenTemplateAndContextAreValid_ShouldCreateEmailMessage()
    {
        // Arrange

        var template = CreateTemplate(ValidBodyHtml);
        var context = CreateContext();
        var composer = CreateComposer();

        // Act

        var result = composer.Compose(template, context);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.FromEmail.Should().Be("account@yaeay.com");
        result.Value.FromName.Should().Be("YaeaY Account");
        result.Value.ToEmail.Should().Be("person@example.com");
        result.Value.Subject.Should().Be("Confirm your YaeaY account");
        result.Value.GetBodyHtml().Should().Contain(
            "https://account.example.com/confirm-email#token=raw-confirmation-token");
        result.Value.GetBodyHtml().Should().NotContain("{{");
    }

    [Fact]
    public void Compose_WhenValuesContainHtmlCharacters_ShouldEncodeInsertedValues()
    {
        // Arrange

        var template = CreateTemplate(ValidBodyHtml);
        var context = new EmailConfirmationMessageContext(
            toEmail: "person@example.com",
            fullName: "Gabriel & Ana",
            rawToken: "token<secret>");
        var composer = CreateComposer();

        // Act

        var result = composer.Compose(template, context);

        // Assert

        result.IsSuccess.Should().BeTrue();
        result.Value.GetBodyHtml().Should().Contain("Gabriel &amp; Ana");
        result.Value.GetBodyHtml().Should().Contain("token%3Csecret%3E");
        result.Value.GetBodyHtml().Should().NotContain("token<secret>");
    }

    [Theory]
    [InlineData(EmailConfirmationMessageComposer.FullNamePlaceholder)]
    [InlineData(EmailConfirmationMessageComposer.ConfirmationUrlPlaceholder)]
    public void Compose_WhenRequiredPlaceholderIsMissing_ShouldFail(string missingPlaceholder)
    {
        // Arrange

        var template = CreateTemplate(
            ValidBodyHtml.Replace(missingPlaceholder, string.Empty, StringComparison.Ordinal));
        var context = CreateContext();
        var composer = CreateComposer();

        // Act

        var result = composer.Compose(template, context);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("email-confirmation-message.placeholder.required");
        result.Error.Message.Should().Contain(missingPlaceholder);
    }

    [Fact]
    public void Compose_WhenBodyContainsUnsupportedPlaceholder_ShouldFail()
    {
        // Arrange

        var template = CreateTemplate($"{ValidBodyHtml}<p>{{{{Unknown}}}}</p>");
        var context = CreateContext();
        var composer = CreateComposer();

        // Act

        var result = composer.Compose(template, context);

        // Assert

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("email-confirmation-message.placeholder.unsupported");
        result.Error.Message.Should().Contain("{{Unknown}}");
    }

    [Fact]
    public void ToString_WhenMessageContainsRawToken_ShouldNotRevealSensitiveContent()
    {
        // Arrange

        var template = CreateTemplate(ValidBodyHtml);
        var context = CreateContext();
        var composer = CreateComposer();

        // Act

        var result = composer.Compose(template, context);

        // Assert

        result.Value.ToString().Should().Be(nameof(EmailMessage));
        result.Value.ToString().Should().NotContain(context.RevealRawToken());
        context.ToString().Should().Be(nameof(EmailConfirmationMessageContext));
        context.ToString().Should().NotContain(context.RevealRawToken());
    }

    private static EmailConfirmationTemplate CreateTemplate(string bodyHtml)
    {
        var fromEmail = Email.Create("account@yaeay.com");

        return EmailConfirmationTemplate.Create(
            fromEmail: fromEmail.Value,
            fromName: "YaeaY Account",
            subject: "Confirm your YaeaY account",
            bodyHtml: bodyHtml,
            isActive: true);
    }

    private static EmailConfirmationMessageContext CreateContext()
        => new(
            toEmail: "person@example.com",
            fullName: "Example Person",
            rawToken: "raw-confirmation-token");

    private static EmailConfirmationMessageComposer CreateComposer()
        => new(new StubEmailConfirmationLinkBuilder());

    private sealed class StubEmailConfirmationLinkBuilder
        : IEmailConfirmationLinkBuilder
    {
        public string Build(string rawToken)
            => $"https://account.example.com/confirm-email#token={Uri.EscapeDataString(rawToken)}";
    }
}
