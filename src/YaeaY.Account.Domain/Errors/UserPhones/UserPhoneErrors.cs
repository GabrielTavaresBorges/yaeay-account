using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.UserPhones;

public static class UserPhoneErrors
{
    public static readonly Error NumberRequired = new(
        Code: "user-phone.number.required",
        Message: "A user phone must have a number.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error VerifiedAtInvalid = new(
        Code: "user-phone.verified-at.invalid",
        Message: "The user phone verification date is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
