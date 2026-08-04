using PhoneNumbers;
using YaeaY.Account.Application.Services.TelephoneNumbers.Errors;
using YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;
using YaeaY.Account.Application.Services.TelephoneNumbers.Models;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber.Mappings;

namespace YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber;

internal sealed class LibPhoneNumberService : ITelephoneNumberService
{
    private readonly PhoneNumberUtil _phoneNumberUtil;

    public LibPhoneNumberService()
    {
        _phoneNumberUtil = PhoneNumberUtil.GetInstance();
    }

    public Result<TelephoneNumberIdentification> ValidateAndIdentify(
        string callingCode,
        string regionCode,
        string? areaCode,
        string number,
        TelephoneType expectedPhoneType)
    {
        var callingCodeValidation = ValidateCallingCode(callingCode);

        if (callingCodeValidation.IsFailure)
            return Result<TelephoneNumberIdentification>.Failure(callingCodeValidation.Error);

        var normalizedCallingCode = callingCodeValidation.Value;

        var normalizedRegionCode = regionCode
            .Trim()
            .ToUpperInvariant();

        var callingCodeRegionValidation = ValidateCallingCodeForRegion(normalizedCallingCode, normalizedRegionCode);

        if (callingCodeRegionValidation.IsFailure)
            return Result<TelephoneNumberIdentification>.Failure(callingCodeRegionValidation.Error);

        var validationResult = ValidateTelephoneNumber(normalizedCallingCode, normalizedRegionCode, areaCode, number);

        if (validationResult.IsFailure)
            return Result<TelephoneNumberIdentification>.Failure(validationResult.Error);

        var telephoneNumberIdentificationResult = IdentifyValidatedTelephoneNumber(validationResult.Value, expectedPhoneType);

        return telephoneNumberIdentificationResult;
    }

    private Result<PhoneNumber> ValidateTelephoneNumber(int callingCode, string regionCode, string? areaCode, string number)
    {
        var telephoneNumberToParse = BuildTelephoneNumber(callingCode, areaCode, number);

        PhoneNumber identifiedTelephoneNumber;

        try
        {
            identifiedTelephoneNumber = _phoneNumberUtil.Parse(telephoneNumberToParse, regionCode);
        }
        catch (NumberParseException exception)
        {
            var error = LibPhoneNumberErrorMap.FromParseException(exception);

            return Result<PhoneNumber>.Failure(error);
        }

        if (identifiedTelephoneNumber.CountryCode != callingCode)
            return Result<PhoneNumber>.Failure(TelephoneNumberIdentificationErrors.CallingCodeDoesNotMatchNumber);

        var possibilityValidation = _phoneNumberUtil.IsPossibleNumberWithReason(identifiedTelephoneNumber);

        var possibilityError = LibPhoneNumberErrorMap.FromValidationResult(possibilityValidation);

        if (possibilityError is not null)
            return Result<PhoneNumber>.Failure(possibilityError);

        if (!_phoneNumberUtil.IsValidNumberForRegion(identifiedTelephoneNumber, regionCode))
            return Result<PhoneNumber>.Failure(TelephoneNumberIdentificationErrors.NumberInvalidForRegion);

        return Result<PhoneNumber>.Success(identifiedTelephoneNumber);
    }

    private Result<int> ValidateCallingCode(string callingCode)
    {
        if (string.IsNullOrWhiteSpace(callingCode))
            return Result<int>.Failure(TelephoneNumberIdentificationErrors.CallingCodeInvalid);

        var normalizedCallingCode = callingCode.Trim();

        if (normalizedCallingCode.StartsWith('+'))
        {
            normalizedCallingCode = normalizedCallingCode[1..];
        }

        var hasValidFormat = normalizedCallingCode.Length > 0 && normalizedCallingCode.All(char.IsDigit);

        if (!hasValidFormat ||
            !int.TryParse(normalizedCallingCode, out var parsedCallingCode) ||
            parsedCallingCode <= 0 ||
            _phoneNumberUtil.GetRegionCodesForCountryCode(parsedCallingCode).Count == 0)
        {
            return Result<int>.Failure(TelephoneNumberIdentificationErrors.CallingCodeInvalid);
        }

        return Result<int>.Success(parsedCallingCode);
    }

    private Result<bool> ValidateCallingCodeForRegion(int callingCode, string regionCode)
    {
        var supportedRegions = _phoneNumberUtil.GetRegionCodesForCountryCode(callingCode);

        var callingCodeMatchesRegion = supportedRegions.Any(
            supportedRegion =>
                string.Equals(
                    supportedRegion,
                    regionCode,
                    StringComparison.OrdinalIgnoreCase));

        if (!callingCodeMatchesRegion)
            return Result<bool>.Failure(TelephoneNumberIdentificationErrors.CallingCodeDoesNotMatchRegion);

        return Result<bool>.Success(true);
    }

    private Result<TelephoneNumberIdentification> IdentifyValidatedTelephoneNumber(PhoneNumber identifiedTelephoneNumber, TelephoneType expectedPhoneType)
    {
        var identifiedRegionCode = _phoneNumberUtil.GetRegionCodeForNumber(identifiedTelephoneNumber);

        if (string.IsNullOrWhiteSpace(identifiedRegionCode))
            return Result<TelephoneNumberIdentification>.Failure(TelephoneNumberIdentificationErrors.RegionNotIdentified);

        var libraryTelephoneType = _phoneNumberUtil.GetNumberType(identifiedTelephoneNumber);

        var identifiedTelephoneType = LibPhoneNumberTypeMap.ToTelephoneType(libraryTelephoneType);

        if (identifiedTelephoneType is null)
            return Result<TelephoneNumberIdentification>.Failure(TelephoneNumberIdentificationErrors.TelephoneTypeNotSupported);

        if (identifiedTelephoneType == TelephoneType.Unknown)
            return Result<TelephoneNumberIdentification>.Failure(TelephoneNumberIdentificationErrors.TelephoneTypeNotIdentified);

        if (!MatchesExpectedType(identifiedTelephoneType.Value, expectedPhoneType))
            return Result<TelephoneNumberIdentification>.Failure(TelephoneNumberIdentificationErrors.TelephoneTypeDoesNotMatchExpected);

        var nationalSignificantNumber = _phoneNumberUtil.GetNationalSignificantNumber(identifiedTelephoneNumber);

        var identifiedAreaCode = IdentifyAreaCode(identifiedTelephoneNumber, nationalSignificantNumber);

        var subscriberNumber = ExtractSubscriberNumber(nationalSignificantNumber, identifiedAreaCode);

        var internationalNumber = _phoneNumberUtil.Format(identifiedTelephoneNumber, PhoneNumberFormat.E164);

        var identification = new TelephoneNumberIdentification(
            regionCode: identifiedRegionCode,
            countryCallingCode: identifiedTelephoneNumber.CountryCode,
            areaCode: identifiedAreaCode,
            nationalNumber: subscriberNumber,
            internationalNumber: internationalNumber,
            telephoneType: identifiedTelephoneType.Value);

        return Result<TelephoneNumberIdentification>.Success(identification);
    }

    private static string BuildTelephoneNumber(int callingCode, string? areaCode, string number)
    {
        var normalizedNumber = number.Trim();

        if (normalizedNumber.StartsWith('+'))
            return normalizedNumber;

        var normalizedAreaCode = string.IsNullOrWhiteSpace(areaCode)
            ? string.Empty
            : areaCode.Trim();

        return $"+{callingCode}{normalizedAreaCode}{normalizedNumber}";
    }

    private static bool MatchesExpectedType(TelephoneType identifiedTelephoneType, TelephoneType expectedTelephoneType)
    {
        if (expectedTelephoneType == TelephoneType.Unknown)
            return true;

        if (identifiedTelephoneType == expectedTelephoneType)
            return true;

        return identifiedTelephoneType == TelephoneType.FixedLineOrMobile &&
               expectedTelephoneType is TelephoneType.Mobile or TelephoneType.Landline;
    }

    private static string ExtractSubscriberNumber(string nationalSignificantNumber, string? areaCode)
    {
        if (string.IsNullOrEmpty(areaCode))
            return nationalSignificantNumber;

        return nationalSignificantNumber[areaCode.Length..];
    }

    private string? IdentifyAreaCode(PhoneNumber identifiedTelephoneNumber, string nationalNumber)
    {
        var areaCodeLength = _phoneNumberUtil.GetLengthOfGeographicalAreaCode(identifiedTelephoneNumber);

        if (areaCodeLength <= 0)
            return null;

        if (areaCodeLength >= nationalNumber.Length)
            return null;

        return nationalNumber[..areaCodeLength];
    }
}
