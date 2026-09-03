using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdatePhones;

public sealed record Command(Guid Id, IReadOnlyCollection<PhoneInput>? Phones) : IRequest<Result<Response>>;
public sealed record PhoneInput(Guid? Id, string CallingCode, string RegionCode, string? AreaCode, TelephoneType PhoneType, string PhoneNumber, bool IsPrimary);
