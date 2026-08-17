using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.EmailConfirmationTokens;

public static class EmailConfirmationTokenErrors
{
    public static readonly Error RawTokenRequired = new(
        Code: "email-confirmation-token.raw-token.required",
        Message: "The email confirmation token is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error RawTokenTooLong = new(
        Code: "email-confirmation-token.raw-token.too-long",
        Message: "The email confirmation token exceeds the maximum allowed length.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error NotFound = new(
        Code: "email-confirmation-token.not-found",
        Message: "The email confirmation token was not found.",
        Category: ErrorCategory.NotFound,
        Rule: ErrorRule.NotFound);

    public static readonly Error EmailDoesNotMatchAccount = new(
        Code: "email-confirmation-token.email.does-not-match-account",
        Message: "The token does not belong to the account's current email address.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AccountNotPendingEmailConfirmation = new(
        Code: "email-confirmation-token.account.not-pending-email-confirmation",
        Message: "The account is not pending email confirmation.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error PendingTokenAlreadyExists = new(
        Code: "email-confirmation-token.pending.already-exists",
        Message: "The user already has a pending email confirmation token.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);

    public static readonly Error InitialStageExpired = new(
        Code: "email-confirmation-token.initial-stage.expired",
        Message: "The initial email confirmation stage has expired.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error UserIdRequired = new(
        Code: "email-confirmation-token.user-id.required",
        Message: "The user identifier is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error EmailRequired = new(
        Code: "email-confirmation-token.email.required",
        Message: "The email to be confirmed is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error TokenHashRequired = new(
        Code: "email-confirmation-token.token-hash.required",
        Message: "The token hash is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ExpirationNotAfterCreation = new(
        Code: "email-confirmation-token.expiration.not-after-creation",
        Message: "The token expiration must occur after its creation.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error RequestedByRequired = new(
        Code: "email-confirmation-token.requested-by.required",
        Message: "The token requester is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error RequestedByInvalid = new(
        Code: "email-confirmation-token.requested-by.invalid",
        Message: "The token requester is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error RequestReasonRequired = new(
        Code: "email-confirmation-token.request-reason.required",
        Message: "The token request reason is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error RequestReasonInvalid = new(
        Code: "email-confirmation-token.request-reason.invalid",
        Message: "The token request reason is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error UsedAtRequired = new(
        Code: "email-confirmation-token.used-at.required",
        Message: "The token usage date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error UsedBeforeCreation = new(
        Code: "email-confirmation-token.used-before-creation",
        Message: "The token cannot be used before its creation.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyUsed = new(
        Code: "email-confirmation-token.already-used",
        Message: "The token has already been used.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error Expired = new(
        Code: "email-confirmation-token.expired",
        Message: "The token has expired.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error Invalidated = new(
        Code: "email-confirmation-token.invalidated",
        Message: "The token has been invalidated.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error InvalidationReasonRequired = new(
        Code: "email-confirmation-token.invalidation-reason.required",
        Message: "The token invalidation reason is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error InvalidationReasonInvalid = new(
        Code: "email-confirmation-token.invalidation-reason.invalid",
        Message: "The token invalidation reason is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error InvalidatedAtRequired = new(
        Code: "email-confirmation-token.invalidated-at.required",
        Message: "The token invalidation date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error InvalidatedBeforeCreation = new(
        Code: "email-confirmation-token.invalidated-before-creation",
        Message: "The token cannot be invalidated before its creation.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyInvalidated = new(
        Code: "email-confirmation-token.already-invalidated",
        Message: "The token has already been invalidated.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error UsedTokenCannotBeInvalidated = new(
        Code: "email-confirmation-token.used-token-cannot-be-invalidated",
        Message: "A used token cannot be invalidated.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);
}
