namespace YaeaY.Account.Application.Services.Emails.Models;

public sealed class EmailMessage
{
    private readonly string _bodyHtml;

    public string FromEmail { get; }
    public string FromName { get; }
    public string ToEmail { get; }
    public string Subject { get; }

    public EmailMessage(
        string fromEmail,
        string fromName,
        string toEmail,
        string subject,
        string bodyHtml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromName);
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyHtml);

        FromEmail = fromEmail;
        FromName = fromName;
        ToEmail = toEmail;
        Subject = subject;
        _bodyHtml = bodyHtml;
    }

    public string GetBodyHtml() => _bodyHtml;

    public override string ToString() => nameof(EmailMessage);
}
