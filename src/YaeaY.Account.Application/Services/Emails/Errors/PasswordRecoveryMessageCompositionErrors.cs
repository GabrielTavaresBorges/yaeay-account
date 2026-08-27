using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Application.Services.Emails.Errors;

public static class PasswordRecoveryMessageCompositionErrors
{
    public static Error MissingRequiredPlaceholder(string placeholder) => new(
        Code: "password-recovery-message.placeholder.required",
        Message: $"The active password recovery template must contain the placeholder '{placeholder}'.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.InvariantViolation);

    public static Error UnsupportedPlaceholder(string placeholder) => new(
        Code: "password-recovery-message.placeholder.unsupported",
        Message: $"The active password recovery template contains the unsupported placeholder '{placeholder}'.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.InvalidValue);
}
