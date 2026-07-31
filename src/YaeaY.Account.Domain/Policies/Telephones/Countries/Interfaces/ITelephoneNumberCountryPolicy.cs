using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.Policies.Telephones.Countries.Interfaces;

public interface ITelephoneNumberCountryPolicy
{
    string RegionCode { get; }

    Result<bool> Validate(
        int callingCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164);
}
