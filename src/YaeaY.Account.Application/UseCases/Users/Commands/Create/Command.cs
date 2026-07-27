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
    string RegionCode,
    string? AreaCode,
    TelephoneType PhoneType,
    string PhoneNumber) : IRequest<Result<Response>>
{
}
