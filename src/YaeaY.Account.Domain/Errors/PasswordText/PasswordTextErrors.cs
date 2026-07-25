using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordText;

public static class PasswordTextErrors
{
    public static readonly Error Required = new(
        Code: "account.password-text.required",
        Message: "Password cannot be null, empty or white space.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error TooShort(int currentLength, int minimumLength) => new(
        Code: "account.password-text.too-short",
        Message: $"Password is too short. Current length: {currentLength} characters. " +
                 $"Minimum required length: {minimumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MinimumLength);

    public static readonly Error MissingUppercase = new(
        Code: "account.password-text.missing-uppercase",
        Message: "Password must contain at least one uppercase letter.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error MissingLowercase = new(
        Code: "account.password-text.missing-lowercase",
        Message: "Password must contain at least one lowercase letter.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error MissingDigit = new(
        Code: "account.password-text.missing-digit",
        Message: "Password must contain at least one number.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error MissingSpecialCharacter = new(
        Code: "account.password-text.missing-special-character",
        Message: "Password must contain at least one special character.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static Error TooLong(int currentLength, int maximumLength) => new(
        Code: "account.password-text.too-long",
        Message: $"Password is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);
}
