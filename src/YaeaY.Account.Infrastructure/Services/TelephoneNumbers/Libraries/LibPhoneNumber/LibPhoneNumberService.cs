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
        string regionCode,
        string? areaCode,
        string number,
        TelephoneType expectedPhoneType)
    {
        var validationResult = ValidateTelephoneNumber(
            regionCode,
            areaCode,
            number);

        if (validationResult.IsFailure)
            return Result<TelephoneNumberIdentification>.Failure(
                validationResult.Error);

        var telephoneNumberIdentificationResult = IdentifyValidatedTelephoneNumber(
            validationResult.Value,
            expectedPhoneType);

        return telephoneNumberIdentificationResult;
    }

    private Result<PhoneNumber> ValidateTelephoneNumber(
        string regionCode,
        string? areaCode,
        string number)
    {
        var telephoneNumberToParse = BuildTelephoneNumber(
            areaCode,
            number);

        var normalizedRegionCode = regionCode
            .Trim()
            .ToUpperInvariant();

        PhoneNumber identifiedTelephoneNumber;

        try
        {
            identifiedTelephoneNumber = _phoneNumberUtil.Parse(
                telephoneNumberToParse,
                normalizedRegionCode);
        }
        catch (NumberParseException exception)
        {
            var error =
                LibPhoneNumberErrorMap.FromParseException(exception);

            return Result<PhoneNumber>.Failure(error);
        }

        var possibilityValidation =
            _phoneNumberUtil.IsPossibleNumberWithReason(
                identifiedTelephoneNumber);

        var possibilityError =
            LibPhoneNumberErrorMap.FromValidationResult(
                possibilityValidation);

        if (possibilityError is not null)
            return Result<PhoneNumber>.Failure(possibilityError);

        if (!_phoneNumberUtil.IsValidNumber(identifiedTelephoneNumber))
        {
            return Result<PhoneNumber>.Failure(
                TelephoneNumberIdentificationErrors
                    .NumberInvalidForRegion);
        }

        return Result<PhoneNumber>.Success(
            identifiedTelephoneNumber);
    }

    private Result<TelephoneNumberIdentification> IdentifyValidatedTelephoneNumber(
        PhoneNumber identifiedTelephoneNumber,
        TelephoneType expectedPhoneType)
    {
        var identifiedRegionCode =
            _phoneNumberUtil.GetRegionCodeForNumber(
                identifiedTelephoneNumber);

        if (string.IsNullOrWhiteSpace(identifiedRegionCode))
            return Result<TelephoneNumberIdentification>.Failure(
                TelephoneNumberIdentificationErrors
                    .RegionNotIdentified);

        var libraryTelephoneType =
            _phoneNumberUtil.GetNumberType(
                identifiedTelephoneNumber);

        var identifiedTelephoneType =
            LibPhoneNumberTypeMap.ToTelephoneType(
                libraryTelephoneType);

        if (identifiedTelephoneType is null)
            return Result<TelephoneNumberIdentification>.Failure(
                TelephoneNumberIdentificationErrors
                    .TelephoneTypeNotSupported);

        if (identifiedTelephoneType == TelephoneType.Unknown)
            return Result<TelephoneNumberIdentification>.Failure(
                TelephoneNumberIdentificationErrors
                    .TelephoneTypeNotIdentified);

        if (!MatchesExpectedType(
                identifiedTelephoneType.Value,
                expectedPhoneType))
            return Result<TelephoneNumberIdentification>.Failure(
                TelephoneNumberIdentificationErrors
                    .TelephoneTypeDoesNotMatchExpected);

        var nationalSignificantNumber =
            _phoneNumberUtil.GetNationalSignificantNumber(
                identifiedTelephoneNumber);

        var identifiedAreaCode = IdentifyAreaCode(
            identifiedTelephoneNumber,
            nationalSignificantNumber);

        var subscriberNumber = ExtractSubscriberNumber(
            nationalSignificantNumber,
            identifiedAreaCode);

        var internationalNumber = _phoneNumberUtil.Format(
            identifiedTelephoneNumber,
            PhoneNumberFormat.E164);

        var identification = new TelephoneNumberIdentification(
            regionCode: identifiedRegionCode,
            countryCallingCode: identifiedTelephoneNumber.CountryCode,
            areaCode: identifiedAreaCode,
            nationalNumber: subscriberNumber,
            internationalNumber: internationalNumber,
            telephoneType: identifiedTelephoneType.Value);

        return Result<TelephoneNumberIdentification>.Success(
            identification);
    }

    private static string BuildTelephoneNumber(
        string? areaCode,
        string number)
    {
        var normalizedNumber = number.Trim();

        if (normalizedNumber.StartsWith('+'))
            return normalizedNumber;

        if (string.IsNullOrWhiteSpace(areaCode))
            return normalizedNumber;

        return $"{areaCode.Trim()}{normalizedNumber}";
    }

    private static bool MatchesExpectedType(
        TelephoneType identifiedTelephoneType,
        TelephoneType expectedTelephoneType)
    {
        if (expectedTelephoneType == TelephoneType.Unknown)
            return true;

        if (identifiedTelephoneType == expectedTelephoneType)
            return true;

        return identifiedTelephoneType == TelephoneType.FixedLineOrMobile &&
               expectedTelephoneType is
                   TelephoneType.Mobile or TelephoneType.Landline;
    }

    private static string ExtractSubscriberNumber(
        string nationalSignificantNumber,
        string? areaCode)
    {
        if (string.IsNullOrEmpty(areaCode))
            return nationalSignificantNumber;

        return nationalSignificantNumber[areaCode.Length..];
    }

    private string? IdentifyAreaCode(
        PhoneNumber identifiedTelephoneNumber,
        string nationalNumber)
    {
        var areaCodeLength =
            _phoneNumberUtil.GetLengthOfGeographicalAreaCode(
                identifiedTelephoneNumber);

        if (areaCodeLength <= 0)
            return null;

        if (areaCodeLength >= nationalNumber.Length)
            return null;

        return nationalNumber[..areaCodeLength];
    }
}
