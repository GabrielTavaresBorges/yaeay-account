using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.Errors.SuspensionInfo;

public static class SuspensionInfoErrors
{
    public static readonly Error ReasonRequired = new(
        Code: "account.suspension-info.reason.required",
        Message: "Suspension reason is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error ReasonInvalid(SuspensionReason receivedReason) => new(
        Code: "account.suspension-info.reason.invalid",
        Message: $"Suspension reason '{receivedReason}' is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error ByRequired = new(
        Code: "account.suspension-info.by.required",
        Message: "The actor responsible for the suspension is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error ByInvalid(SuspensionBy receivedBy) => new(
        Code: "account.suspension-info.by.invalid",
        Message: $"Suspension actor '{receivedBy}' is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error SuspendedAtRequired = new(
        Code: "account.suspension-info.suspended-at.required",
        Message: "Suspension date is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static Error NoteTooLong(
        int currentLength,
        int maximumLength) => new(
        Code: "account.suspension-info.note.too-long",
        Message: $"Suspension note is too long. Current length: {currentLength} characters. " +
                 $"Maximum allowed length: {maximumLength} characters.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MaximumLength);

    public static Error SuspendedUntilNotAfterSuspendedAt(
        DateTimeOffset suspendedAt,
        DateTimeOffset suspendedUntil) => new(
        Code: "account.suspension-info.suspended-until.not-after-suspended-at",
        Message: $"Suspension end date must be after the suspension start date. " +
                 $"Suspended at: {suspendedAt:O}. Suspended until: {suspendedUntil:O}.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
