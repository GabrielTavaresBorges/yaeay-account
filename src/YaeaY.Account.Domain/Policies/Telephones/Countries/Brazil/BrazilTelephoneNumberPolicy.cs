using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Policies.Telephones.Countries.Interfaces;

using YaeaY.Account.Domain.Errors.Telephones.Countries.Brazil;

namespace YaeaY.Account.Domain.Policies.Telephones.Countries.Brazil;

internal sealed class BrazilTelephoneNumberPolicy : ITelephoneNumberCountryPolicy
{
    private static readonly IReadOnlyDictionary<string, string> FederativeUnitsByAreaCode =
        new Dictionary<string, string>
        {
            // AC
            ["68"] = "AC",

            // AL
            ["82"] = "AL",

            // AM
            ["92"] = "AM", ["97"] = "AM",

            // AP
            ["96"] = "AP",

            // BA
            ["71"] = "BA", ["73"] = "BA", ["74"] = "BA",
            ["75"] = "BA", ["77"] = "BA",

            // CE
            ["85"] = "CE", ["88"] = "CE",

            // DF/GO
            ["61"] = "DF/GO",

            // ES
            ["27"] = "ES", ["28"] = "ES",

            // GO
            ["62"] = "GO", ["64"] = "GO",

            // MA
            ["98"] = "MA", ["99"] = "MA",

            // MG
            ["31"] = "MG", ["32"] = "MG", ["33"] = "MG",
            ["34"] = "MG", ["35"] = "MG", ["37"] = "MG",
            ["38"] = "MG",

            // MS
            ["67"] = "MS",

            // MT
            ["65"] = "MT", ["66"] = "MT",

            // PA
            ["91"] = "PA", ["93"] = "PA", ["94"] = "PA",

            // PB
            ["83"] = "PB",

            // PE
            ["81"] = "PE", ["87"] = "PE",

            // PI
            ["86"] = "PI", ["89"] = "PI",

            // PR
            ["41"] = "PR", ["42"] = "PR", ["43"] = "PR",
            ["44"] = "PR", ["45"] = "PR", ["46"] = "PR",

            // RJ
            ["21"] = "RJ", ["22"] = "RJ", ["24"] = "RJ",

            // RN
            ["84"] = "RN",

            // RO
            ["69"] = "RO",

            // RR
            ["95"] = "RR",

            // RS
            ["51"] = "RS", ["53"] = "RS", ["54"] = "RS",
            ["55"] = "RS",

            // SC
            ["47"] = "SC", ["48"] = "SC", ["49"] = "SC",

            // SE
            ["79"] = "SE",

            // SP
            ["11"] = "SP", ["12"] = "SP", ["13"] = "SP",
            ["14"] = "SP", ["15"] = "SP", ["16"] = "SP",
            ["17"] = "SP", ["18"] = "SP", ["19"] = "SP",

            // TO
            ["63"] = "TO"
        };

    public string RegionCode => "BR";

    public Result<bool> Validate(
        int callingCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164)
    {
        if (callingCode != 55)
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.CallingCodeInvalid);

        var areaCodeValidation = ValidateAreaCodeFromBrazil(areaCode, phoneType);
        if (areaCodeValidation.IsFailure)
            return areaCodeValidation;

        var validationNationalNumberResult = ValidateNationalNumberFromBrazil(nationalNumber, phoneType);

        var nationalNumberFromBrazil = validationNationalNumberResult.Value;

        return Result<bool>.Success(nationalNumberFromBrazil);
    }

    private static Result<bool> ValidateAreaCodeFromBrazil(string? areaCode, TelephoneType phoneType)
    {
        var requiresGeographicAreaCode = phoneType is
                TelephoneType.Landline or
                TelephoneType.Mobile or
                TelephoneType.FixedLineOrMobile;

        if (requiresGeographicAreaCode && string.IsNullOrWhiteSpace(areaCode))
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.AreaCodeRequired);

        if (areaCode is not null && !FederativeUnitsByAreaCode.ContainsKey(areaCode))
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.AreaCodeInvalid);

        return Result<bool>.Success(true);
    }

    private static Result<bool> ValidateNationalNumberFromBrazil(string nationalNumber, TelephoneType phoneType)
    {
        if (string.IsNullOrWhiteSpace(nationalNumber) || nationalNumber.Any(character => !char.IsDigit(character)))
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.NationalNumberInvalid);

        if (phoneType == TelephoneType.Landline && nationalNumber.Length != 8)
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.LandlineNumberInvalid);

        if (phoneType == TelephoneType.Mobile && (nationalNumber.Length != 9 || nationalNumber[0] != '9'))
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.MobileNumberInvalid);

        if (phoneType is TelephoneType.FixedLineOrMobile or TelephoneType.Voip && nationalNumber.Length is not (8 or 9))
            return Result<bool>.Failure(BrazilTelephoneNumberErrors.NationalNumberInvalid);

        return Result<bool>.Success(true);
    }
}
