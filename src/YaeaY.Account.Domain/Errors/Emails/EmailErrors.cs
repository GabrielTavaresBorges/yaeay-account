using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Emails;

public static class EmailErrors
{
    public static readonly Error Required = new(
        Code: "account.email.required",
        Message: "Email is required. Please provide an address in the format 'example@domain.com'.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error TooLong(int currentLength, int maximumLength) => new(
        Code: "account.email.too-long",
        Message: $"Email is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error InvalidFormat = new(
        Code: "account.email.invalid-format",
        Message: "Email format is invalid. Please provide an address in the format 'example@domain.com'.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);
}
