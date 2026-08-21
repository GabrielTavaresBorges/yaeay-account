using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Application.Services.Emails.Errors;

public static class PasswordRecoveryMessageCompositionErrors
{
    public static Error MissingRequiredPlaceholder(string placeholder) => new(
        "password-recovery-message.placeholder.required",
        $"The active password recovery template must contain the placeholder '{placeholder}'.",
        ErrorCategory.Unexpected,
        ErrorRule.InvariantViolation);

    public static Error UnsupportedPlaceholder(string placeholder) => new(
        "password-recovery-message.placeholder.unsupported",
        $"The active password recovery template contains the unsupported placeholder '{placeholder}'.",
        ErrorCategory.Unexpected,
        ErrorRule.InvalidValue);
}
