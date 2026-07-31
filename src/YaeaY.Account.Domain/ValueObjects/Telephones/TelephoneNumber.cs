using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.ValueObjects.Telephones;

public sealed record TelephoneNumber
{
    private readonly string _callingCode = string.Empty;
    private readonly string _regionCode = string.Empty;
    private readonly string? _areaCode;
    private readonly TelephoneType _phoneType;
    private readonly string _nationalNumber = string.Empty;
    private readonly string _e164 = string.Empty;

    public string CallingCode => _callingCode;
    public string RegionCode => _regionCode;
    public string? AreaCode => _areaCode;
    public TelephoneType PhoneType => _phoneType;
    public string NationalNumber => _nationalNumber;
    public string E164 => _e164;

    private TelephoneNumber() { }

    private TelephoneNumber(
        string callingCode,
        string regionCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164)
    {
        _callingCode = callingCode;
        _regionCode = regionCode;
        _areaCode = areaCode;
        _phoneType = phoneType;
        _nationalNumber = nationalNumber;
        _e164 = e164;
    }

    public static Result<TelephoneNumber> Create(
        string callingCode,
        string regionCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164)
    {
        var telephoneNumber = new TelephoneNumber(
            callingCode,
            regionCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        return Result<TelephoneNumber>.Success(telephoneNumber);
    }
}
