using System.Net;
using System.Text.RegularExpressions;
using YaeaY.Account.Application.Services.Emails.Errors;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;

namespace YaeaY.Account.Application.Services.Emails;

public sealed class EmailConfirmationMessageComposer(
    IEmailConfirmationLinkBuilder confirmationLinkBuilder)
{
    public const string FullNamePlaceholder = "{{FullName}}";
    public const string ConfirmationUrlPlaceholder = "{{ConfirmationUrl}}";

    private static readonly IReadOnlyCollection<string> RequiredPlaceholders =
    [
        FullNamePlaceholder,
        ConfirmationUrlPlaceholder
    ];

    private static readonly Regex PlaceholderPattern = new(
        pattern: "\\{\\{[^{}]+\\}\\}",
        options: RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromMilliseconds(100));

    public Result<EmailMessage> Compose(
        EmailConfirmationTemplate template,
        EmailConfirmationMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var placeholder in RequiredPlaceholders)
        {
            if (!template.BodyHtml.Contains(placeholder, StringComparison.Ordinal))
            {
                return Result<EmailMessage>.Failure(
                    EmailConfirmationMessageCompositionErrors.MissingRequiredPlaceholder(placeholder));
            }
        }

        var unsupportedSubjectPlaceholder = PlaceholderPattern.Match(template.Subject);
        if (unsupportedSubjectPlaceholder.Success)
        {
            return Result<EmailMessage>.Failure(
                EmailConfirmationMessageCompositionErrors.UnsupportedPlaceholder(
                    unsupportedSubjectPlaceholder.Value));
        }

        var confirmationUrl = confirmationLinkBuilder.Build(
            context.RevealRawToken());

        var bodyHtml = template.BodyHtml
            .Replace(
                FullNamePlaceholder,
                WebUtility.HtmlEncode(context.FullName),
                StringComparison.Ordinal)
            .Replace(
                ConfirmationUrlPlaceholder,
                WebUtility.HtmlEncode(confirmationUrl),
                StringComparison.Ordinal);

        var unsupportedBodyPlaceholder = PlaceholderPattern.Match(bodyHtml);
        if (unsupportedBodyPlaceholder.Success)
        {
            return Result<EmailMessage>.Failure(
                EmailConfirmationMessageCompositionErrors.UnsupportedPlaceholder(
                    unsupportedBodyPlaceholder.Value));
        }

        return Result<EmailMessage>.Success(
            new EmailMessage(
                fromEmail: template.FromEmail.EmailAddress,
                fromName: template.FromName,
                toEmail: context.ToEmail,
                subject: template.Subject,
                bodyHtml: bodyHtml));
    }

}
