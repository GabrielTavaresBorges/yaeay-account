using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;

public static class PasswordRecoveryChallengeErrors
{
    public static readonly Error UserIdRequired = new(
        Code: "password-recovery-challenge.user-id.required",
        Message: "The user identifier is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error EmailRequired = new(
        Code: "password-recovery-challenge.email.required",
        Message: "The recovery email is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error RequestedAtRequired = new(
        Code: "password-recovery-challenge.requested-at.required",
        Message: "The request date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error CodeHashRequired = new(
        Code: "password-recovery-challenge.code-hash.required",
        Message: "The recovery code hash is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error IssuedAtRequired = new(
        Code: "password-recovery-challenge.issued-at.required",
        Message: "The code issuance date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ExpirationNotAfterIssue = new(
        Code: "password-recovery-challenge.expiration.not-after-issue",
        Message: "The recovery code expiration must occur after issuance.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyIssued = new(
        Code: "password-recovery-challenge.already-issued",
        Message: "The recovery code has already been issued.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error NotIssued = new(
        Code: "password-recovery-challenge.not-issued",
        Message: "The recovery code has not been issued.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error InvalidOrExpired = new(
        Code: "password-recovery-challenge.invalid-or-expired",
        Message: "The recovery code is invalid or expired.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyVerified = new(
        Code: "password-recovery-challenge.already-verified",
        Message: "The recovery code has already been verified.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyConsumed = new(
        Code: "password-recovery-challenge.already-consumed",
        Message: "The recovery authorization has already been consumed.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyInvalidated = new(
        Code: "password-recovery-challenge.already-invalidated",
        Message: "The recovery challenge has already been invalidated.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AuthorizationExpirationInvalid = new(
        Code: "password-recovery-challenge.authorization-expiration.invalid",
        Message: "The reset authorization expiration is invalid.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error ResetNotAuthorized = new(
        Code: "password-recovery-challenge.reset.not-authorized",
        Message: "Password reset is not authorized or has expired.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error InvalidationReasonRequired = new(
        Code: "password-recovery-challenge.invalidation-reason.required",
        Message: "The invalidation reason is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error InvalidationReasonInvalid = new(
        Code: "password-recovery-challenge.invalidation-reason.invalid",
        Message: "The invalidation reason is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error InvalidatedAtRequired = new(
        Code: "password-recovery-challenge.invalidated-at.required",
        Message: "The invalidation date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error MaximumAttemptsInvalid = new(
        Code: "password-recovery-challenge.maximum-attempts.invalid",
        Message: "The maximum number of attempts must be positive.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error RequestTemporarilyLimited = new(
        Code: "password-recovery.request.temporarily-limited",
        Message: "A new recovery request cannot be issued yet.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.InvariantViolation);
}
