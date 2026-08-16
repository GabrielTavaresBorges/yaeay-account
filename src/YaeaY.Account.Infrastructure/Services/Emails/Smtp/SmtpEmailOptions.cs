namespace YaeaY.Account.Infrastructure.Services.Emails.Smtp;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "EmailDelivery:Smtp";

    public bool IsActive { get; init; }
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public SmtpSecurityMode SecurityMode { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int TimeoutInSeconds { get; init; }
}
