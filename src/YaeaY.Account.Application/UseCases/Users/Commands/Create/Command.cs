using MediatR;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed record Command(
    string EmailAddress,
    string Password,
    string FullName,
    DateOnly BirthDate,
    Gender Gender,
    string CallingCode,
    string RegionCode,
    string? AreaCode,
    PhoneType PhoneType,
    string PhoneNumber,
    string E164) : IRequest<Result<Response>>
{
}
