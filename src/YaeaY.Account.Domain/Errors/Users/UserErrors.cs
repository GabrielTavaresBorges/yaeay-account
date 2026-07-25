

using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Users;

public static class UserErrors
{
    public static readonly Error EmailRequired = new(
        Code: "user.email.required",
        Message: "A user must have an email address.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PasswordRequired = new(
        Code: "user.password.required",
        Message: "A user must have a password.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error FullNameRequired = new(
        Code: "user.full-name.required",
        Message: "A user must have a full name.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error BirthDateRequired = new(
        Code: "user.birth-date.required",
        Message: "A user must have a birth date.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error GenderRequired = new(
        Code: "user.gender.required",
        Message: "A user must have a defined gender.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error GenderInvalid = new(
        Code: "user.gender.invalid",
        Message: "The informed gender is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error PhoneRequired = new(
        Code: "user.phone.required",
        Message: "A phone must be informed.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PhoneAlreadyExists = new(
        Code: "user.phone.already-exists",
        Message: "The phone is already associated with this user.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);

    public static readonly Error AtLeastOnePhoneRequired = new(
        Code: "user.phone.at-least-one-required",
        Message: "A user must have at least one phone.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error PrimaryPhoneRequired = new(
        Code: "user.phone.primary-required",
        Message: "A user must have exactly one primary phone.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error PrimaryPhoneCannotBeRemoved = new(
        Code: "user.phone.primary-cannot-be-removed",
        Message: "The primary phone cannot be removed before another phone is set as primary.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error PhoneNotFound = new(
        Code: "user.phone.not-found",
        Message: "The phone is not associated with this user.",
        Category: ErrorCategory.NotFound,
        Rule: ErrorRule.NotFound);
}
