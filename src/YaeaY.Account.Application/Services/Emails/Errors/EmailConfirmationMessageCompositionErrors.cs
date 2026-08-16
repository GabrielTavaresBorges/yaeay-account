using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Application.Services.Emails.Errors;

public static class EmailConfirmationMessageCompositionErrors
{
    public static Error MissingRequiredPlaceholder(string placeholder) => new(
        Code: "email-confirmation-message.placeholder.required",
        Message: $"The active email confirmation template must contain the placeholder '{placeholder}'.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.InvariantViolation);

    public static Error UnsupportedPlaceholder(string placeholder) => new(
        Code: "email-confirmation-message.placeholder.unsupported",
        Message: $"The active email confirmation template contains the unsupported placeholder '{placeholder}'.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.InvalidValue);
}
