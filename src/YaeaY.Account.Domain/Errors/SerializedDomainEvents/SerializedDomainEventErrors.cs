using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.SerializedDomainEvents;

public static class SerializedDomainEventErrors
{
    public static readonly Error EventTypeRequired = new(
        Code: "serialized-domain-event.event-type.required",
        Message: "The domain event type is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error EventTypeTooLong(int currentLength, int maximumLength) => new(
        Code: "serialized-domain-event.event-type.too-long",
        Message: $"The domain event type is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static readonly Error PayloadRequired = new(
        Code: "serialized-domain-event.payload.required",
        Message: "The serialized domain event payload is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PayloadInvalid = new(
        Code: "serialized-domain-event.payload.invalid",
        Message: "The serialized domain event payload must contain valid JSON.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);
}
