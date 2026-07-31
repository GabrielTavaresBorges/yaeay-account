using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Application.Services.TelephoneNumbers.Errors;

public static class TelephoneNumberIdentificationErrors
{
    public static readonly Error CallingCodeInvalid = new(
        Code: "account.telephone-number.identification.calling-code.invalid",
        Message: "The telephone calling code is invalid.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error CallingCodeDoesNotMatchRegion = new(
        Code: "account.telephone-number.identification.calling-code.region-mismatch",
        Message: "The telephone calling code does not match the specified region.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error CallingCodeDoesNotMatchNumber = new(
        Code: "account.telephone-number.identification.calling-code.number-mismatch",
        Message: "The telephone calling code does not match the identified telephone number.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error NumberNotIdentified = new(
       Code: "account.telephone-number.identification.not-identified",
       Message: "The telephone number could not be identified.",
       Category: ErrorCategory.Validation,
       Rule: ErrorRule.InvalidFormat);

    public static readonly Error NumberTooShortForRegion = new(
        Code: "account.telephone-number.identification.too-short-for-region",
        Message: "The telephone number is too short for the specified region.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error NumberTooLongForRegion = new(
        Code: "account.telephone-number.identification.too-long-for-region",
        Message: "The telephone number is too long for the specified region.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error NumberInvalidForRegion = new(
        Code: "account.telephone-number.identification.invalid-for-region",
        Message: "The telephone number is not valid for the identified region.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error RegionNotIdentified = new(
        Code: "account.telephone-number.identification.region.not-identified",
        Message: "The telephone number region could not be identified.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error TelephoneTypeNotIdentified = new(
        Code: "account.telephone-number.identification.type.not-identified",
        Message: "The telephone number type could not be identified.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error TelephoneTypeNotSupported = new(
        Code: "account.telephone-number.identification.type.not-supported",
        Message: "The identified telephone number type is not supported.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);

    public static readonly Error TelephoneTypeDoesNotMatchExpected = new(
        Code: "account.telephone-number.identification.type.unexpected",
        Message: "The identified telephone number type does not match the expected telephone number type.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
