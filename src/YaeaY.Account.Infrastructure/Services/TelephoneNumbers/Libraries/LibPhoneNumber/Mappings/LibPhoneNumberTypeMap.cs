using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber.Mappings;

internal static class LibPhoneNumberTypeMap
{
    public static TelephoneType? ToTelephoneType(
        PhoneNumbers.PhoneNumberType libraryTelephoneType)
    {
        return libraryTelephoneType switch
        {
            PhoneNumbers.PhoneNumberType.FIXED_LINE =>
                TelephoneType.Landline,

            PhoneNumbers.PhoneNumberType.MOBILE =>
                TelephoneType.Mobile,

            PhoneNumbers.PhoneNumberType.FIXED_LINE_OR_MOBILE =>
                TelephoneType.FixedLineOrMobile,

            PhoneNumbers.PhoneNumberType.VOIP =>
                TelephoneType.Voip,

            PhoneNumbers.PhoneNumberType.UNKNOWN =>
                TelephoneType.Unknown,

            PhoneNumbers.PhoneNumberType.TOLL_FREE or
            PhoneNumbers.PhoneNumberType.PREMIUM_RATE or
            PhoneNumbers.PhoneNumberType.SHARED_COST or
            PhoneNumbers.PhoneNumberType.PERSONAL_NUMBER or
            PhoneNumbers.PhoneNumberType.PAGER or
            PhoneNumbers.PhoneNumberType.UAN or
            PhoneNumbers.PhoneNumberType.VOICEMAIL =>
                null,

            _ =>
                null
        };
    }
}
