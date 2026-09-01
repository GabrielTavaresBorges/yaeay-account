using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.UserDocuments;

public static class UserDocumentErrors
{
    public static readonly Error CpfRequired = new(
        Code: "user-document.cpf.required",
        Message: "A CPF document must contain a valid CPF.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ImageRequired = new(
        Code: "user-document.image.required",
        Message: "A document image cannot be null.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error ImageLimitExceeded = new(
        Code: "user-document.image.limit-exceeded",
        Message: "A document can contain at most five images.",
        Category: ErrorCategory.BusinessRule,
        Rule: ErrorRule.InvariantViolation);

    public static readonly Error CpfImagesMinimumRequired = new(
        Code: "user-document.cpf.images.minimum-required",
        Message: "A CPF document must contain at least three images.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.MinimumLength);

    public static readonly Error CpfSingleCurrentRequired = new(
        Code: "user-document.cpf.single-current-required",
        Message: "Only one current CPF document can be updated at a time.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error ImagePositionAlreadyExists = new(
        Code: "user-document.image.position.already-exists",
        Message: "The document already contains an image at the informed position.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);

    public static readonly Error ImageStorageObjectKeyAlreadyExists = new(
        Code: "user-document.image.storage-object-key.already-exists",
        Message: "The storage object key is already associated with a document image.",
        Category: ErrorCategory.Conflict,
        Rule: ErrorRule.AlreadyExists);
}
