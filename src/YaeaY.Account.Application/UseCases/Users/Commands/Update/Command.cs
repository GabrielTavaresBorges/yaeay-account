using MediatR;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Dates;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Update;

public sealed record Command(
    Guid Id,
    string? Email,
    string? Password,
    string? FullName,
    BirthDate? BirthDate
    ) : IRequest<Result<Response>>
{

}
