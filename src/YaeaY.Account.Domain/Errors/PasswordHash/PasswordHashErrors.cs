using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordHash;

public static class PasswordHashErrors
{
    public static readonly Error Required = new(
        Code: "account.password-hash.required",
        Message: "Password hash cannot be null, empty or white space.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error TooLong(int currentLength, int maximumLength) => new(
        Code: "account.password-hash.too-long",
        Message: $"Password hash is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);
}
