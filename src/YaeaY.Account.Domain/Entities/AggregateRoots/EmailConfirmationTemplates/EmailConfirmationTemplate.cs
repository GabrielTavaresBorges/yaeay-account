using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Errors.EmailConfirmationSettings;
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
        string bodyHtml)
    {
        if (fromEmail is null)
            throw new DomainException(EmailConfirmationSettingErrors.FromEmailRequired);

        if (string.IsNullOrWhiteSpace(fromName))
            throw new DomainException(EmailConfirmationSettingErrors.FromNameRequired);

        if (fromName.Trim().Length > 150)
            throw new DomainException(EmailConfirmationSettingErrors.FromNameTooLong);

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(EmailConfirmationSettingErrors.SubjectRequired);

        if (subject.Trim().Length > 200)
            throw new DomainException(EmailConfirmationSettingErrors.SubjectTooLong);

        if (string.IsNullOrWhiteSpace(bodyHtml))
            throw new DomainException(EmailConfirmationSettingErrors.BodyHtmlRequired);
    }
}
