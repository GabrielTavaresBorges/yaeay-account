using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Presentation.Server.Contracts.Users;

public sealed record UpdatePhonesRequest(IReadOnlyCollection<UpdatePhoneRequest>? Phones);
public sealed record UpdatePhoneRequest(Guid? Id, string CallingCode, string RegionCode, string? AreaCode, TelephoneType PhoneType, string PhoneNumber, bool IsPrimary);
