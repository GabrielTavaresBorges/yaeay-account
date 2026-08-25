using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordRecoveryTemplates;

public static class PasswordRecoveryTemplateErrors
{
    public static readonly Error FromEmailRequired = new(
        Code: "password-recovery-template.from-email.required",
        Message: "From email is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error FromNameRequired = new(
        Code: "password-recovery-template.from-name.required",
        Message: "From name is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error FromNameTooLong = new(
        Code: "password-recovery-template.from-name.too-long",
        Message: "From name cannot be longer than 150 characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error SubjectRequired = new(
        Code: "password-recovery-template.subject.required",
        Message: "Subject is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error SubjectTooLong = new(
        Code: "password-recovery-template.subject.too-long",
        Message: "Subject cannot be longer than 200 characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error BodyHtmlRequired = new(
        Code: "password-recovery-template.body-html.required",
        Message: "Body HTML is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PurposeRequired = new(
        Code: "password-recovery-template.purpose.required",
        Message: "Template purpose is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PurposeInvalid = new(
        Code: "password-recovery-template.purpose.invalid",
        Message: "Template purpose is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
