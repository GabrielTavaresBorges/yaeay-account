using FluentAssertions;
using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Infrastructure.Services.Emails.Smtp;

namespace YaeaY.Account.Infrastructure.UnitTests.Services.Emails.Smtp;

public sealed class HostingerSmtpEmailSenderSendAsyncTests
{
    [Fact]
    public async Task SendAsync_WhenSmtpDeliveryIsInactive_ShouldRejectSending()
    {
        // Arrange

        var sender = CreateSender(CreateOptions(isActive: false, password: string.Empty));
        var message = CreateMessage();

        // Act

        Func<Task> act = () => sender.SendAsync(message);

        // Assert

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("SMTP email delivery is inactive.");
    }

    [Fact]
    public async Task SendAsync_WhenPasswordIsMissing_ShouldRejectSending()
    {
        // Arrange

        var sender = CreateSender(CreateOptions(isActive: true, password: string.Empty));
        var message = CreateMessage();

        // Act

        Func<Task> act = () => sender.SendAsync(message);

        // Assert

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The SMTP password is not configured.");
    }

    [Fact]
    public async Task SendAsync_WhenMessageSenderDiffersFromSmtpAccount_ShouldRejectSending()
    {
        // Arrange

        var sender = CreateSender(CreateOptions(isActive: true, password: "test-password"));
        var message = new EmailMessage(
            fromEmail: "another@yaeay.com",
            fromName: "YaeaY Account",
            toEmail: "person@example.com",
            subject: "Confirm your account",
            bodyHtml: "<p>Confirmation</p>");

        // Act

        Func<Task> act = () => sender.SendAsync(message);

        // Assert

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The message sender must match the configured SMTP account.");
    }

    private static HostingerSmtpEmailSender CreateSender(SmtpEmailOptions options)
        => new(Options.Create(options));

    private static SmtpEmailOptions CreateOptions(bool isActive, string password)
        => new()
        {
            IsActive = isActive,
            Host = "smtp.hostinger.com",
            Port = 587,
            SecurityMode = SmtpSecurityMode.StartTls,
            Username = "account@yaeay.com",
            Password = password,
            TimeoutInSeconds = 30
        };

    private static EmailMessage CreateMessage()
        => new(
            fromEmail: "account@yaeay.com",
            fromName: "YaeaY Account",
            toEmail: "person@example.com",
            subject: "Confirm your account",
            bodyHtml: "<p>Confirmation</p>");
}
