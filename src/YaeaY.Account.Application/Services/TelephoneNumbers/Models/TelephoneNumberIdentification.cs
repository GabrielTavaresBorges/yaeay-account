using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.Services.TelephoneNumbers.Models;

public sealed class TelephoneNumberIdentification
{
    public string RegionCode { get; }

    public int CountryCallingCode { get; }

    public string? AreaCode { get; }

    public string NationalNumber { get; }

    public string InternationalNumber { get; }

    public TelephoneType TelephoneType { get; }

    public TelephoneNumberIdentification(
        string regionCode,
        int countryCallingCode,
        string? areaCode,
        string nationalNumber,
        string internationalNumber,
        TelephoneType telephoneType)
    {
        RegionCode = regionCode;
        CountryCallingCode = countryCallingCode;
        AreaCode = areaCode;
        NationalNumber = nationalNumber;
        InternationalNumber = internationalNumber;
        TelephoneType = telephoneType;
    }
}
