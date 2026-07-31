using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Telephones;

namespace YaeaY.Account.Domain.Factories.Telephones;

public interface ITelephoneNumberFactory
{
    Result<TelephoneNumber> Create(
        int callingCode,
        string regionCode,
        string? areaCode,
        TelephoneType phoneType,
        string nationalNumber,
        string e164);
}
