using YaeaY.Account.Application.Services.TelephoneNumbers.Models;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;

public interface ITelephoneNumberService
{
    Result<TelephoneNumberIdentification> ValidateAndIdentify(
        string regionCode,
        string? areaCode,
        string number,
        TelephoneType expectedPhoneType);
}
