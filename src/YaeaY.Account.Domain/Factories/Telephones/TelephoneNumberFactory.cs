using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Telephones;
using YaeaY.Account.Domain.Policies.Telephones.Countries.Interfaces;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.Factories.Telephones;

public sealed class TelephoneNumberFactory : ITelephoneNumberFactory
{
    private readonly IReadOnlyDictionary<string, ITelephoneNumberCountryPolicy> _countryPolicies;

    public TelephoneNumberFactory(IEnumerable<ITelephoneNumberCountryPolicy> countryPolicies)
    {
        _countryPolicies = countryPolicies.ToDictionary(policy => policy.RegionCode, StringComparer.OrdinalIgnoreCase);
    }

    public Result<TelephoneNumber> Create(
         int callingCode,
         string regionCode,
         string? areaCode,
         TelephoneType phoneType,
         string nationalNumber,
         string e164)
    {
        if (!_countryPolicies.TryGetValue(regionCode, out var countryPolicy))
            return Result<TelephoneNumber>.Failure(TelephoneNumberErrors.CountryNotSupported);

        var countryValidation = countryPolicy.Validate(
            callingCode,
            areaCode,
            phoneType,
            nationalNumber,
            e164);

        if (countryValidation.IsFailure)
            return Result<TelephoneNumber>.Failure(countryValidation.Error);

        var telephoneNumberResult = TelephoneNumber.Create(
            callingCode: $"+{callingCode}",
            regionCode: regionCode,
            areaCode: areaCode,
            phoneType: phoneType,
            nationalNumber: nationalNumber,
            e164: e164);

        var telephoneNumber = telephoneNumberResult.Value;

        return Result<TelephoneNumber>.Success(telephoneNumber);
    }
}
