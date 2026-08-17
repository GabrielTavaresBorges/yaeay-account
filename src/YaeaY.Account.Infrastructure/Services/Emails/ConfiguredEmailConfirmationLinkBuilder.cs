using Microsoft.Extensions.Options;
using YaeaY.Account.Application.Services.Emails.Interfaces;

namespace YaeaY.Account.Infrastructure.Services.Emails;

public sealed class ConfiguredEmailConfirmationLinkBuilder
    : IEmailConfirmationLinkBuilder
{
    private readonly Uri _confirmationPageUri;

    public ConfiguredEmailConfirmationLinkBuilder(
        IOptions<EmailConfirmationLinkOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var confirmationPageUrl = options.Value.ConfirmationPageUrl;
        if (!IsValidConfirmationPageUrl(confirmationPageUrl))
        {
            throw new InvalidOperationException(
                "EmailConfirmationLink:ConfirmationPageUrl must be an absolute HTTPS URL without query or fragment.");
        }

        _confirmationPageUri = new Uri(
            confirmationPageUrl,
            UriKind.Absolute);
    }

    public string Build(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        var uriBuilder = new UriBuilder(_confirmationPageUri)
        {
            Fragment = $"token={Uri.EscapeDataString(rawToken)}"
        };

        return uriBuilder.Uri.AbsoluteUri;
    }

    public static bool IsValidConfirmationPageUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && uri.Scheme == Uri.UriSchemeHttps
           && string.IsNullOrEmpty(uri.UserInfo)
           && string.IsNullOrEmpty(uri.Query)
           && string.IsNullOrEmpty(uri.Fragment);
}
