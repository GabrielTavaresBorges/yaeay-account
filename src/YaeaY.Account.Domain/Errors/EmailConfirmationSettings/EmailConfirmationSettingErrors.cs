using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.EmailConfirmationSettings;

public static class EmailConfirmationSettingErrors
{
    public static readonly Error FromEmailRequired = new(
        Code: "email-confirmation-setting.from-email.required",
        Message: "From email is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error FromNameRequired = new(
        Code: "email-confirmation-setting.from-name.required",
        Message: "From name is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error FromNameTooLong = new(
        Code: "email-confirmation-setting.from-name.too-long",
        Message: "From name cannot be longer than 150 characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error SubjectRequired = new(
        Code: "email-confirmation-setting.subject.required",
        Message: "Subject is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error SubjectTooLong = new(
        Code: "email-confirmation-setting.subject.too-long",
        Message: "Subject cannot be longer than 200 characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error BodyHtmlRequired = new(
        Code: "email-confirmation-setting.body-html.required",
        Message: "Body HTML is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);
}
