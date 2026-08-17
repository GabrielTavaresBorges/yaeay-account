namespace YaeaY.Account.Application.Services.Emails.Models;

public sealed class EmailConfirmationMessageContext
{
    private readonly string _rawToken;

    public string ToEmail { get; }
    public string FullName { get; }

    public EmailConfirmationMessageContext(
        string toEmail,
        string fullName,
        string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        ToEmail = toEmail;
        FullName = fullName;
        _rawToken = rawToken;
    }

    public string RevealRawToken() => _rawToken;

    public override string ToString() => nameof(EmailConfirmationMessageContext);
}
