using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordRecoveryTemplates;

public static class PasswordRecoveryTemplateErrors
{
    public static readonly Error FromEmailRequired = Required("from-email", "From email is required.");
    public static readonly Error FromNameRequired = Required("from-name", "From name is required.");
    public static readonly Error FromNameTooLong = Maximum("from-name", "From name cannot be longer than 150 characters.");
    public static readonly Error SubjectRequired = Required("subject", "Subject is required.");
    public static readonly Error SubjectTooLong = Maximum("subject", "Subject cannot be longer than 200 characters.");
    public static readonly Error BodyHtmlRequired = Required("body-html", "Body HTML is required.");
    public static readonly Error PurposeRequired = Required("purpose", "Template purpose is required.");
    public static readonly Error PurposeInvalid = new(
        "password-recovery-template.purpose.invalid", "Template purpose is invalid.", ErrorCategory.Validation, ErrorRule.InvalidValue);

    private static Error Required(string field, string message) =>
        new($"password-recovery-template.{field}.required", message, ErrorCategory.Validation, ErrorRule.Required);

    private static Error Maximum(string field, string message) =>
        new($"password-recovery-template.{field}.too-long", message, ErrorCategory.Validation, ErrorRule.MaximumLength);
}
