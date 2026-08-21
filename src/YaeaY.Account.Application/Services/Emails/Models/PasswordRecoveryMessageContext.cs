namespace YaeaY.Account.Application.Services.Emails.Models;

public sealed class PasswordRecoveryMessageContext(string toEmail, string fullName, string? rawCode, DateTimeOffset? changedAtUtc)
{
    private readonly string? _rawCode = rawCode;
    public string ToEmail { get; } = toEmail;
    public string FullName { get; } = fullName;
    public DateTimeOffset? ChangedAtUtc { get; } = changedAtUtc;
    public string RevealRawCode() => _rawCode ?? throw new InvalidOperationException("No recovery code is available in this message context.");
    public override string ToString() => nameof(PasswordRecoveryMessageContext);
}
