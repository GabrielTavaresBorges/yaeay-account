using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.PhoneNumbers;

public static class PhoneNumberErrors
{
    public static readonly Error CallingCodeRequired = new(
        "phone-number.calling-code.required", "The phone calling code is required.",
        ErrorCategory.Validation, ErrorRule.Required);

    public static readonly Error CallingCodeInvalid = new(
        "phone-number.calling-code.invalid", "The phone calling code must be in the format +<digits>.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error RegionCodeInvalid = new(
        "phone-number.region-code.invalid", "The phone region code must be a two-letter ISO code.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error AreaCodeInvalid = new(
        "phone-number.area-code.invalid", "The phone area code must contain digits only.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error PhoneTypeRequired = new(
        "phone-number.type.required", "The phone type is required.",
        ErrorCategory.Validation, ErrorRule.Required);

    public static readonly Error PhoneTypeInvalid = new(
        "phone-number.type.invalid", "The phone type is invalid.",
        ErrorCategory.Validation, ErrorRule.InvalidValue);

    public static readonly Error PhoneTypeDoesNotMatch = new(
        "phone-number.type.does-not-match", "The informed phone type does not match the number.",
        ErrorCategory.Validation, ErrorRule.InvalidValue);

    public static readonly Error PhoneTypeNotSupported = new(
        "phone-number.type.not-supported", "The detected phone type is not supported.",
        ErrorCategory.Validation, ErrorRule.InvalidValue);

    public static readonly Error NationalNumberRequired = new(
        "phone-number.national-number.required", "The phone national number is required.",
        ErrorCategory.Validation, ErrorRule.Required);

    public static readonly Error NationalNumberInvalid = new(
        "phone-number.national-number.invalid", "The phone national number must contain digits only.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error InvalidForRegion = new(
        "phone-number.invalid-for-region", "The phone number is not valid for the informed region.",
        ErrorCategory.Validation, ErrorRule.InvalidValue);

    public static readonly Error InvalidFormat = new(
        "phone-number.invalid-format", "The phone number could not be interpreted.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error E164Invalid = new(
        "phone-number.e164.invalid", "The E.164 phone number is invalid.",
        ErrorCategory.Validation, ErrorRule.InvalidFormat);

    public static readonly Error CanonicalDataInconsistent = new(
        "phone-number.canonical-data.inconsistent", "The canonical phone number data is inconsistent.",
        ErrorCategory.Validation, ErrorRule.InvariantViolation);
}
