using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.FullName;

public static class FullNameErrors
{
    public static readonly Error Required = new(
        Code: "account.full-name.required",
        Message: "Full name cannot be null, empty or white space.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error TooShort(
        int currentLength,
        int minimumLength) => new(
        Code: "account.full-name.too-short",
        Message: $"Full name is too short. Current length: {currentLength} characters. " +
                 $"Minimum required length: {minimumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MinimumLength);

    public static Error TooLong(
        int currentLength,
        int maximumLength) => new(
        Code: "account.full-name.too-long",
        Message: $"Full name is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);
}
