using PhoneNumbers;
using YaeaY.Account.Application.Services.TelephoneNumbers.Errors;
using YaeaY.Account.Domain.Abstraction.Errors;

namespace YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber.Mappings;

internal static class LibPhoneNumberErrorMap
{
    public static Error FromParseException(
        NumberParseException exception)
    {
        return exception.ErrorType switch
        {
            ErrorType.INVALID_COUNTRY_CODE =>
                TelephoneNumberIdentificationErrors.RegionNotIdentified,

            ErrorType.TOO_SHORT_AFTER_IDD or
            ErrorType.TOO_SHORT_NSN =>
                TelephoneNumberIdentificationErrors.NumberTooShortForRegion,

            ErrorType.TOO_LONG =>
                TelephoneNumberIdentificationErrors.NumberTooLongForRegion,

            ErrorType.NOT_A_NUMBER =>
                TelephoneNumberIdentificationErrors.NumberNotIdentified,

            _ =>
                TelephoneNumberIdentificationErrors.NumberNotIdentified
        };
    }

    public static Error? FromValidationResult(
        PhoneNumberUtil.ValidationResult validationResult)
    {
        return validationResult switch
        {
            PhoneNumberUtil.ValidationResult.IS_POSSIBLE =>
                null,

            PhoneNumberUtil.ValidationResult.TOO_SHORT =>
                TelephoneNumberIdentificationErrors.NumberTooShortForRegion,

            PhoneNumberUtil.ValidationResult.TOO_LONG =>
                TelephoneNumberIdentificationErrors.NumberTooLongForRegion,

            PhoneNumberUtil.ValidationResult.INVALID_COUNTRY_CODE =>
                TelephoneNumberIdentificationErrors.RegionNotIdentified,

            PhoneNumberUtil.ValidationResult.INVALID_LENGTH or
            PhoneNumberUtil.ValidationResult.IS_POSSIBLE_LOCAL_ONLY =>
                TelephoneNumberIdentificationErrors.NumberNotIdentified,

            _ =>
                TelephoneNumberIdentificationErrors.NumberNotIdentified
        };
    }
}
