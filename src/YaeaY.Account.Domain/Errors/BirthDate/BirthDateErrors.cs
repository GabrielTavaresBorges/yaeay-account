using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.BirthDate;

public static class BirthDateErrors
{
    public static Error InFuture(
        DateOnly receivedDate,
        DateOnly currentDate) => new(
        Code: "account.birth-date.in-future",
        Message: $"Birth date cannot be in the future. " +
                 $"Received date: {receivedDate:yyyy-MM-dd}. " +
                 $"Current date (UTC): {currentDate:yyyy-MM-dd}.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static Error TooOld(
        DateOnly receivedDate,
        DateOnly minimumAllowedDate,
        int maximumAgeYears) => new(
        Code: "account.birth-date.too-old",
        Message: $"Birth date cannot represent an age greater than {maximumAgeYears} years. " +
                 $"Received date: {receivedDate:yyyy-MM-dd}. " +
                 $"Minimum allowed date (UTC): {minimumAllowedDate:yyyy-MM-dd}.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
