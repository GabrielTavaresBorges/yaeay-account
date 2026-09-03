using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateBasicData;

public sealed record Command(Guid Id, string? FullName, DateOnly? BirthDate, Gender? Gender)
    : IRequest<Result<Response>>;
