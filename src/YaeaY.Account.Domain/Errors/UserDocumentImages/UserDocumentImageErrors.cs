using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.UserDocumentImages;

public static class UserDocumentImageErrors
{
    public static readonly Error PositionInvalid = new("user-document-image.position.invalid", "The image position must be between 1 and 5.", ErrorCategory.Validation, ErrorRule.InvalidValue);
    public static readonly Error StorageObjectKeyRequired = new("user-document-image.storage-object-key.required", "The storage object key is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error StorageObjectKeyTooLong = new("user-document-image.storage-object-key.too-long", "The storage object key cannot exceed 512 characters.", ErrorCategory.Validation, ErrorRule.MaximumLength);
    public static readonly Error OriginalFileNameRequired = new("user-document-image.original-file-name.required", "The original file name is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error OriginalFileNameTooLong = new("user-document-image.original-file-name.too-long", "The original file name cannot exceed 255 characters.", ErrorCategory.Validation, ErrorRule.MaximumLength);
    public static readonly Error ContentTypeRequired = new("user-document-image.content-type.required", "The image content type is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error ContentTypeTooLong = new("user-document-image.content-type.too-long", "The image content type cannot exceed 100 characters.", ErrorCategory.Validation, ErrorRule.MaximumLength);
    public static readonly Error FileSizeInvalid = new("user-document-image.file-size.invalid", "The image file size must be greater than zero.", ErrorCategory.Validation, ErrorRule.InvalidValue);
    public static readonly Error Sha256HashRequired = new("user-document-image.sha256-hash.required", "The SHA-256 hash is required.", ErrorCategory.Validation, ErrorRule.Required);
    public static readonly Error Sha256HashInvalid = new("user-document-image.sha256-hash.invalid", "The SHA-256 hash must contain exactly 64 hexadecimal characters.", ErrorCategory.Validation, ErrorRule.InvalidFormat);
}
