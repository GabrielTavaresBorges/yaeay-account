using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;

public sealed class PasswordRecoveryTemplate : Entity, IAggregateRoot
{
    private readonly PasswordRecoveryTemplatePurpose _purpose;
    private readonly Email _fromEmail = null!;
    private readonly string _fromName = string.Empty;
    private readonly string _subject = string.Empty;
    private readonly string _bodyHtml = string.Empty;
    private readonly bool _isActive;
    private readonly DateTimeOffset _updatedAt;

    public PasswordRecoveryTemplatePurpose Purpose => _purpose;
    public Email FromEmail => _fromEmail;
    public string FromName => _fromName;
    public string Subject => _subject;
    public string BodyHtml => _bodyHtml;
    public bool IsActive => _isActive;
    public DateTimeOffset UpdatedAt => _updatedAt;

    private PasswordRecoveryTemplate() { }

    private PasswordRecoveryTemplate(
        PasswordRecoveryTemplatePurpose purpose,
        Email fromEmail,
        string fromName,
        string subject,
        string bodyHtml,
        bool isActive)
    {
        _purpose = purpose;
        _fromEmail = fromEmail;
        _fromName = fromName.Trim();
        _subject = subject.Trim();
        _bodyHtml = bodyHtml;
        _isActive = isActive;
        _updatedAt = DateTimeOffset.UtcNow;
    }

    public static PasswordRecoveryTemplate Create(
        PasswordRecoveryTemplatePurpose purpose,
        Email fromEmail,
        string fromName,
        string subject,
        string bodyHtml,
        bool isActive = true)
    {
        if (purpose == PasswordRecoveryTemplatePurpose.Unknown)
            throw new DomainException(PasswordRecoveryTemplateErrors.PurposeRequired);

        if (!Enum.IsDefined(purpose))
            throw new DomainException(PasswordRecoveryTemplateErrors.PurposeInvalid);

        if (fromEmail is null)
            throw new DomainException(PasswordRecoveryTemplateErrors.FromEmailRequired);

        if (string.IsNullOrWhiteSpace(fromName))
            throw new DomainException(PasswordRecoveryTemplateErrors.FromNameRequired);

        if (fromName.Trim().Length > 150)
            throw new DomainException(PasswordRecoveryTemplateErrors.FromNameTooLong);

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException(PasswordRecoveryTemplateErrors.SubjectRequired);

        if (subject.Trim().Length > 200)
            throw new DomainException(PasswordRecoveryTemplateErrors.SubjectTooLong);

        if (string.IsNullOrWhiteSpace(bodyHtml))
            throw new DomainException(PasswordRecoveryTemplateErrors.BodyHtmlRequired);

        return new PasswordRecoveryTemplate(purpose, fromEmail, fromName, subject, bodyHtml, isActive);
    }
}
