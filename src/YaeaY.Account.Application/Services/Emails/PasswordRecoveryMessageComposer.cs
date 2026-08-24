using System.Net;
using System.Text.RegularExpressions;
using YaeaY.Account.Application.Services.Emails.Errors;
using YaeaY.Account.Application.Services.Emails.Models;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.Services.Emails;

public sealed class PasswordRecoveryMessageComposer
{
    public const string FullNamePlaceholder = "{{FullName}}";
    public const string RecoveryCodePlaceholder = "{{RecoveryCode}}";
    public const string ChangedAtUtcPlaceholder = "{{ChangedAtUtc}}";

    private static readonly Regex PlaceholderPattern = new("\\{\\{[^{}]+\\}\\}", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public Result<EmailMessage> Compose(PasswordRecoveryTemplate template, PasswordRecoveryMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(context);

        var required = template.Purpose switch
        {
            PasswordRecoveryTemplatePurpose.RecoveryCode => new[] { FullNamePlaceholder, RecoveryCodePlaceholder },
            PasswordRecoveryTemplatePurpose.PasswordChanged => new[] { FullNamePlaceholder, ChangedAtUtcPlaceholder },
            _ => Array.Empty<string>()
        };

        foreach (var placeholder in required)
        {
            if (!template.BodyHtml.Contains(placeholder, StringComparison.Ordinal))
                return Result<EmailMessage>.Failure(PasswordRecoveryMessageCompositionErrors.MissingRequiredPlaceholder(placeholder));
        }

        var unsupportedSubject = PlaceholderPattern.Match(template.Subject);
        if (unsupportedSubject.Success)
            return Result<EmailMessage>.Failure(PasswordRecoveryMessageCompositionErrors.UnsupportedPlaceholder(unsupportedSubject.Value));

        var body = template.BodyHtml.Replace(FullNamePlaceholder, WebUtility.HtmlEncode(context.FullName), StringComparison.Ordinal);
        body = template.Purpose switch
        {
            PasswordRecoveryTemplatePurpose.RecoveryCode => body.Replace(RecoveryCodePlaceholder, WebUtility.HtmlEncode(context.RevealRawCode()), StringComparison.Ordinal),
            PasswordRecoveryTemplatePurpose.PasswordChanged => body.Replace(ChangedAtUtcPlaceholder, WebUtility.HtmlEncode(context.ChangedAtUtc!.Value.ToString("u")), StringComparison.Ordinal),
            _ => body
        };

        var unsupportedBody = PlaceholderPattern.Match(body);
        if (unsupportedBody.Success)
            return Result<EmailMessage>.Failure(PasswordRecoveryMessageCompositionErrors.UnsupportedPlaceholder(unsupportedBody.Value));

        return Result<EmailMessage>.Success(new EmailMessage(
            template.FromEmail.EmailAddress, template.FromName, context.ToEmail, template.Subject, body));
    }
}
