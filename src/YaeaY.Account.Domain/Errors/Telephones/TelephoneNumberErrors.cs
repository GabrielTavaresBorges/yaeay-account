using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Telephones;

public static class TelephoneNumberErrors
{
    public static readonly Error CallingCodeRequired = new(
        Code: "phone-number.calling-code.required",
        Message: "The phone calling code is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error CallingCodeInvalid = new(
        Code: "phone-number.calling-code.invalid",
        Message: "The phone calling code must be in the format +<digits>.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error RegionCodeRequired = new(
        Code: "phone-number.region-code.required",
        Message: "The phone region code is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error RegionCodeInvalid = new(
        Code: "phone-number.region-code.invalid",
        Message: "The phone region code must be a two-letter ISO code.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error AreaCodeInvalid = new(
        Code: "phone-number.area-code.invalid",
        Message: "The phone area code must contain digits only.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error PhoneTypeRequired = new(
        Code: "phone-number.type.required",
        Message: "The phone type is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error PhoneTypeInvalid = new(
        Code: "phone-number.type.invalid",
        Message: "The phone type is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error PhoneTypeDoesNotMatch = new(
        Code: "phone-number.type.does-not-match",
        Message: "The informed phone type does not match the number.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error PhoneTypeNotSupported = new(
        Code: "phone-number.type.not-supported",
        Message: "The detected phone type is not supported.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error NationalNumberRequired = new(
        Code: "phone-number.national-number.required",
        Message: "The phone national number is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error NationalNumberInvalid = new(
        Code: "phone-number.national-number.invalid",
        Message: "The phone national number must contain digits only.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error InvalidForRegion = new(
        Code: "phone-number.invalid-for-region",
        Message: "The phone number is not valid for the informed region.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error InvalidFormat = new(
        Code: "phone-number.invalid-format",
        Message: "The phone number could not be interpreted.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error E164Required = new(
        Code: "phone-number.e164.required",
        Message: "The E.164 phone number is required.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error E164Invalid = new(
        Code: "phone-number.e164.invalid",
        Message: "The E.164 phone number is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error DataInconsistent = new(
        Code: "phone-number.data.inconsistent",
        Message: "The phone number data is inconsistent.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvariantViolation);
}
