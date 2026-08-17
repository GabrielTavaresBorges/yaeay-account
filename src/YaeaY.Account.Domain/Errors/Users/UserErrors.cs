

using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Users;

public static class UserErrors
{
    public static readonly Error EmailConfirmationDateRequired = new(
        Code: "user.email-confirmation.date.required",
        Message: "The email confirmation date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error EmailConfirmationBeforeAccountCreation = new(
        Code: "user.email-confirmation.before-account-creation",
        Message: "The email cannot be confirmed before the account was created.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error EmailAlreadyConfirmed = new(
        Code: "user.email.already-confirmed",
        Message: "The account email has already been confirmed.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);

    public static readonly Error AccountCannotBeEmailConfirmed = new(
        Code: "user.account.cannot-be-email-confirmed",
        Message: "The account cannot be confirmed in its current state.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error SuspensionPreventsEmailConfirmation = new(
        Code: "user.suspension.prevents-email-confirmation",
        Message: "Only an automatic inactivity suspension can be removed by email confirmation.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error NotFound = new(
        Code: "user.not-found",
        Message: "The user was not found.",
        Category: ErrorCategory.NotFound,
        Rule: ErrorRule.NotFound);

    public static readonly Error EmailAlreadyInUse = new(
        Code: "user.email.already-in-use",
        Message: "The email address is already associated with another user.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);

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

    public static readonly Error LoginDateRequired = new(
        Code: "user.login.date.required",
        Message: "The login date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error AccountCannotLogin = new(
        Code: "user.account.cannot-login",
        Message: "The account cannot login in its current state.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error EmailConfirmationRequiredForLogin = new(
        Code: "user.login.email-confirmation-required",
        Message: "The email address must be confirmed before login.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error SuspendedAccountCannotLogin = new(
        Code: "user.login.account-suspended",
        Message: "A suspended account cannot login.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error DisabledAccountCannotLogin = new(
        Code: "user.login.account-disabled",
        Message: "A disabled account cannot login.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error LoginBeforePreviousAccountActivity = new(
        Code: "user.login.before-previous-account-activity",
        Message: "The login date cannot be before the previous account activity.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

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
