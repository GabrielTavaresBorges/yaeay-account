using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.Services.TelephoneNumbers.Models;

public sealed class TelephoneNumberIdentification
{
    public int CallingCode { get; }
    public string RegionCode { get; }

    public string? AreaCode { get; }

    public string NationalNumber { get; }

    public string InternationalNumber { get; }

    public TelephoneType TelephoneType { get; }

    public TelephoneNumberIdentification(
        int countryCallingCode,
        string regionCode,
        string? areaCode,
        string nationalNumber,
        string internationalNumber,
        TelephoneType telephoneType)
    {
        CallingCode = countryCallingCode;
        RegionCode = regionCode;
        AreaCode = areaCode;
        NationalNumber = nationalNumber;
        InternationalNumber = internationalNumber;
        TelephoneType = telephoneType;
    }
}
