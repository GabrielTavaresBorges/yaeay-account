using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.OutboxMessages;

public static class OutboxMessageErrors
{
    public static readonly Error IdRequired = new(
        Code: "outbox-message.id.required",
        Message: "The outbox message identifier is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ContentRequired = new(
        Code: "outbox-message.content.required",
        Message: "The serialized domain event is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error OccurredOnUtcRequired = new(
        Code: "outbox-message.occurred-on-utc.required",
        Message: "The domain event occurrence date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error AlreadyProcessed = new(
        Code: "outbox-message.already-processed",
        Message: "The outbox message has already been processed.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error ProcessedOnUtcRequired = new(
        Code: "outbox-message.processed-on-utc.required",
        Message: "The processing date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ProcessedBeforeOccurrence = new(
        Code: "outbox-message.processed-before-occurrence",
        Message: "The outbox message cannot be processed before the domain event occurred.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error FailureRequired = new(
        Code: "outbox-message.failure.required",
        Message: "The processing failure description is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error AttemptedOnUtcRequired = new(
        Code: "outbox-message.attempted-on-utc.required",
        Message: "The processing attempt date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error AttemptedBeforeOccurrence = new(
        Code: "outbox-message.attempted-before-occurrence",
        Message: "The outbox message cannot be attempted before the domain event occurred.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error NextAttemptOnUtcRequired = new(
        Code: "outbox-message.next-attempt-on-utc.required",
        Message: "The next processing attempt date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error NextAttemptNotAfterAttempt = new(
        Code: "outbox-message.next-attempt-not-after-attempt",
        Message: "The next processing attempt must occur after the current attempt.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error MustBeProcessedBeforePublication = new(
        Code: "outbox-message.must-be-processed-before-publication",
        Message: "The outbox message must be processed locally before publication.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error AlreadyPublished = new(
        Code: "outbox-message.already-published",
        Message: "The outbox message has already been published.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error PublishedOnUtcRequired = new(
        Code: "outbox-message.published-on-utc.required",
        Message: "The publication date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PublishedBeforeOccurrence = new(
        Code: "outbox-message.published-before-occurrence",
        Message: "The outbox message cannot be published before the domain event occurred.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);
}
