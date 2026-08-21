using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;

public static class PasswordRecoveryChallengeErrors
{
    public static readonly Error UserIdRequired = Validation("user-id.required", "The user identifier is required.", ErrorRule.Required);
    public static readonly Error EmailRequired = Validation("email.required", "The recovery email is required.", ErrorRule.Required);
    public static readonly Error RequestedAtRequired = Validation("requested-at.required", "The request date is required.", ErrorRule.Required);
    public static readonly Error CodeHashRequired = Validation("code-hash.required", "The recovery code hash is required.", ErrorRule.Required);
    public static readonly Error IssuedAtRequired = Validation("issued-at.required", "The code issuance date is required.", ErrorRule.Required);
    public static readonly Error ExpirationNotAfterIssue = Business("expiration.not-after-issue", "The recovery code expiration must occur after issuance.");
    public static readonly Error AlreadyIssued = Business("already-issued", "The recovery code has already been issued.");
    public static readonly Error NotIssued = Business("not-issued", "The recovery code has not been issued.");
    public static readonly Error InvalidOrExpired = Business("invalid-or-expired", "The recovery code is invalid or expired.");
    public static readonly Error AlreadyVerified = Business("already-verified", "The recovery code has already been verified.");
    public static readonly Error AlreadyConsumed = Business("already-consumed", "The recovery authorization has already been consumed.");
    public static readonly Error AlreadyInvalidated = Business("already-invalidated", "The recovery challenge has already been invalidated.");
    public static readonly Error AuthorizationExpirationInvalid = Business("authorization-expiration.invalid", "The reset authorization expiration is invalid.");
    public static readonly Error ResetNotAuthorized = Business("reset.not-authorized", "Password reset is not authorized or has expired.");
    public static readonly Error InvalidationReasonRequired = Validation("invalidation-reason.required", "The invalidation reason is required.", ErrorRule.Required);
    public static readonly Error InvalidationReasonInvalid = Validation("invalidation-reason.invalid", "The invalidation reason is invalid.", ErrorRule.InvalidValue);
    public static readonly Error InvalidatedAtRequired = Validation("invalidated-at.required", "The invalidation date is required.", ErrorRule.Required);
    public static readonly Error MaximumAttemptsInvalid = Validation("maximum-attempts.invalid", "The maximum number of attempts must be positive.", ErrorRule.InvalidValue);
    public static readonly Error RequestTemporarilyLimited = new(
        "password-recovery.request.temporarily-limited",
        "A new recovery request cannot be issued yet.",
        ErrorCategory.Conflict,
        ErrorRule.InvariantViolation);

    private static Error Validation(string suffix, string message, ErrorRule rule) =>
        new($"password-recovery-challenge.{suffix}", message, ErrorCategory.Validation, rule);

    private static Error Business(string suffix, string message) =>
        new($"password-recovery-challenge.{suffix}", message, ErrorCategory.BusinessRule, ErrorRule.InvariantViolation);
}
