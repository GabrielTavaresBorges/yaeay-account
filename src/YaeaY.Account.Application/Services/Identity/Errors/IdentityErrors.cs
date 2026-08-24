using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Application.Services.Identity.Errors;

public static class IdentityErrors
{
    public static readonly Error CreationFailed = new(
        Code: "identity.account.creation-failed",
        Message: "The account credential could not be created.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.Unexpected);

    public static readonly Error NotFound = new(
        Code: "identity.account.not-found",
        Message: "The account credential was not found.",
        Category: ErrorCategory.NotFound,
        Rule: ErrorRule.NotFound);

    public static readonly Error EmailConfirmationFailed = new(
        Code: "identity.email.confirmation-failed",
        Message: "The credential email could not be confirmed.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.Unexpected);

    public static readonly Error InvalidCredentials = new(
        Code: "identity.credentials.invalid",
        Message: "The email or password is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error LockedOut = new(
        Code: "identity.account.locked-out",
        Message: "The account is temporarily locked because of failed access attempts.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error SignInFailed = new(
        Code: "identity.session.creation-failed",
        Message: "The authenticated session could not be created.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.Unexpected);

    public static readonly Error PasswordResetFailed = new(
        Code: "identity.password.reset-failed",
        Message: "The account password could not be reset.",
        Category: ErrorCategory.Unexpected,
        Rule: ErrorRule.Unexpected);
}
