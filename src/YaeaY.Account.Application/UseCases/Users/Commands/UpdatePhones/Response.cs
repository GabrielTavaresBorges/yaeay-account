using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdatePhones;

public sealed record Response(Guid Id, IReadOnlyCollection<PhoneResponse> Phones, bool HasChanges, string Message);
public sealed record PhoneResponse(Guid Id, string CallingCode, string RegionCode, string? AreaCode, TelephoneType PhoneType, string PhoneNumber, bool IsPrimary);
