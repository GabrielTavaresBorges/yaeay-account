namespace YaeaY.Account.Infrastructure.Services.Emails;

public sealed class EmailConfirmationLinkOptions
{
    public const string SectionName = "EmailConfirmationLink";

    public string ConfirmationPageUrl { get; init; } = string.Empty;
}
