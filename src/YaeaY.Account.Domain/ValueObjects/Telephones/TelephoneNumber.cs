using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Telephones;

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
        var normalizedCallingCode = (callingCode ?? string.Empty).Trim();
        var normalizedRegionCode = (regionCode ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedAreaCode = string.IsNullOrWhiteSpace(areaCode) ? null : areaCode.Trim();
        var normalizedNationalNumber = (nationalNumber ?? string.Empty).Trim();
        var normalizedE164 = (e164 ?? string.Empty).Trim();

        var validateTelephoneNumber = ValidateTelephoneNumber(
            normalizedCallingCode,
            normalizedRegionCode,
            normalizedAreaCode,
            phoneType,
            normalizedNationalNumber,
            normalizedE164);

        if (validateTelephoneNumber.IsFailure)
            return Result<TelephoneNumber>.Failure(validateTelephoneNumber.Error);

        var telephoneNumber = new TelephoneNumber(
            normalizedCallingCode,
            normalizedRegionCode,
            normalizedAreaCode,
            phoneType,
            normalizedNationalNumber,
            normalizedE164);

        return Result<TelephoneNumber>.Success(telephoneNumber);
    }

    private static Result<bool> ValidateTelephoneNumber(
        string callingCode,
        string regionCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
            return Result<bool>.Failure(TelephoneNumberErrors.CallingCodeRequired);

        if (!callingCode.StartsWith('+') || callingCode.Length < 2 || callingCode[1..].Any(character => !char.IsDigit(character)))
            return Result<bool>.Failure(TelephoneNumberErrors.CallingCodeInvalid);

        if (string.IsNullOrWhiteSpace(regionCode))
            return Result<bool>.Failure(TelephoneNumberErrors.RegionCodeRequired);

        if (regionCode.Length != 2 || regionCode.Any(character => character is < 'A' or > 'Z'))
            return Result<bool>.Failure(TelephoneNumberErrors.RegionCodeInvalid);

        if (areaCode is not null && areaCode.Any(character => !char.IsDigit(character)))
            return Result<bool>.Failure(TelephoneNumberErrors.AreaCodeInvalid);

        if (phoneType == TelephoneType.Unknown)
            return Result<bool>.Failure(TelephoneNumberErrors.PhoneTypeRequired);

        if (!Enum.IsDefined(phoneType))
            return Result<bool>.Failure(TelephoneNumberErrors.PhoneTypeInvalid);

        if (string.IsNullOrWhiteSpace(nationalNumber))
            return Result<bool>.Failure(TelephoneNumberErrors.NationalNumberRequired);

        if (nationalNumber.Any(character => !char.IsDigit(character)))
            return Result<bool>.Failure(TelephoneNumberErrors.NationalNumberInvalid);

        if (string.IsNullOrWhiteSpace(e164))
            return Result<bool>.Failure(TelephoneNumberErrors.E164Required);

        if (!e164.StartsWith('+') || e164.Length < 2 || e164[1..].Any(character => !char.IsDigit(character)))
            return Result<bool>.Failure(TelephoneNumberErrors.E164Invalid);

        var expectedE164 = $"{callingCode}{areaCode}{nationalNumber}";
        if (!string.Equals(e164, expectedE164, StringComparison.Ordinal))
            return Result<bool>.Failure(TelephoneNumberErrors.DataInconsistent);

        return Result<bool>.Success(true);
    }
}

