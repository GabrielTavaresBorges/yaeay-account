using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;

namespace YaeaY.Account.Infrastructure.Services.Emails.Smtp;

public sealed class HostingerSmtpEmailSender : IEmailSender
{
    private const int MaximumProviderResponseLogLength = 512;
    private static readonly EventId SmtpEmailAcceptedEvent = new(1001, "SmtpEmailAccepted");

    private readonly SmtpEmailOptions _options;
    private readonly ILogger<HostingerSmtpEmailSender> _logger;

    public HostingerSmtpEmailSender(
        IOptions<SmtpEmailOptions> options,
        ILogger<HostingerSmtpEmailSender> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.IsActive)
            throw new InvalidOperationException("SMTP email delivery is inactive.");

        if (string.IsNullOrWhiteSpace(_options.Password))
            throw new InvalidOperationException("The SMTP password is not configured.");

        if (!string.Equals(
                message.FromEmail,
                _options.Username,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The message sender must match the configured SMTP account.");
        }

        var mimeMessage = CreateMimeMessage(message);
        mimeMessage.MessageId ??= MimeUtils.GenerateMessageId();

        using var smtpClient = new SmtpClient
        {
            Timeout = checked(_options.TimeoutInSeconds * 1000)
        };

        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            ResolveSecureSocketOptions(_options.SecurityMode),
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            _options.Username,
            _options.Password,
            cancellationToken);

        var providerResponse = await smtpClient.SendAsync(mimeMessage, cancellationToken);

        _logger.LogInformation(
            SmtpEmailAcceptedEvent,
            "SMTP server accepted email {MessageId}. Provider response: {ProviderResponse}",
            mimeMessage.MessageId,
            NormalizeProviderResponse(providerResponse));

        await smtpClient.DisconnectAsync(quit: true, cancellationToken);
    }

    private static MimeMessage CreateMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(
            new MailboxAddress(message.FromName, message.FromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(message.ToEmail));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new BodyBuilder
        {
            HtmlBody = message.GetBodyHtml()
        }.ToMessageBody();

        return mimeMessage;
    }

    private static SecureSocketOptions ResolveSecureSocketOptions(
        SmtpSecurityMode securityMode)
        => securityMode switch
        {
            SmtpSecurityMode.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => throw new InvalidOperationException(
                $"Unsupported SMTP security mode '{securityMode}'.")
        };

    private static string NormalizeProviderResponse(string providerResponse)
    {
        var normalized = providerResponse
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        return normalized.Length <= MaximumProviderResponseLogLength
            ? normalized
            : normalized[..MaximumProviderResponseLogLength];
    }
}
