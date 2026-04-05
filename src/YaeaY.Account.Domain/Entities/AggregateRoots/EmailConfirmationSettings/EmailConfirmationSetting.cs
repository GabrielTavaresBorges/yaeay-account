using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Records;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;

public sealed class EmailConfirmationSetting : Entity, IAggregateRoot
{
    private readonly Email _fromEmail = null!;
    private readonly string _fromName = string.Empty;
    private readonly string _subject = string.Empty;
    private readonly string _bodyHtml = string.Empty;
    private readonly bool _isActive;
    private readonly DateTimeOffset _updatedAt;

    public Email FromEmail => _fromEmail;
    public string FromName => _fromName;
    public string Subject => _subject;
    public string BodyHtml => _bodyHtml;
    public bool IsActive => _isActive;
    public DateTimeOffset UpdatedAt => _updatedAt;

    private EmailConfirmationSetting() { }

    private EmailConfirmationSetting(
        Email fromEmail,
        string fromName,
        string subject,
        string bodyHtml,
        bool isActive)
    {
        _fromEmail = fromEmail;
        _fromName = fromName;
        _subject = subject;
        _bodyHtml = bodyHtml;
        _isActive = isActive;
        _updatedAt = DateTimeOffset.UtcNow;
    }

    public static EmailConfirmationSetting Create(
        Email fromEmail,
        string fromName,
        string subject,
        string bodyHtml,
        bool isActive = true)
    {
        Validate(fromEmail, fromName, subject, bodyHtml);

        var emailConfirmationSetting = new EmailConfirmationSetting(
            fromEmail,
            fromName,
            subject,
            bodyHtml,
            isActive = true);

        return emailConfirmationSetting;
    }

    private static void Validate(
        Email fromEmail,
        string fromName,
        string subject,
        string bodyHtml)    {
        
        if (string.IsNullOrWhiteSpace(fromName))
            throw new DomainException(
                identifier: "EMAIL_CONFIRMATION_SETTING_FROM_NAME_NULL_EMPTY_WHITE_SPACE",
                message: "From name cannot be null, empty or white space.");

        if (fromName.Trim().Length > 150)
            throw new DomainException(
                identifier: "EMAIL_CONFIRMATION_SETTING_FROM_NAME_TOO_LONG",
                message: "From name cannot be longer than 150 characters.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(
                identifier: "EMAIL_CONFIRMATION_SETTING_SUBJECT_NULL_EMPTY_WHITE_SPACE",
                message: "Subject cannot be null, empty or white space.");

        if (subject.Trim().Length > 200)
            throw new DomainException(
                message: "Subject cannot be longer than 200 characters.",
                identifier: "EMAIL_CONFIRMATION_SETTING_SUBJECT_TOO_LONG");

        if (string.IsNullOrWhiteSpace(bodyHtml))
            throw new DomainException(
                identifier: "EMAIL_CONFIRMATION_SETTING_BODY_NULL_EMPTY_WHITE_SPACE",
                message: "Body HTML cannot be null, empty or white space." );
    }
}
