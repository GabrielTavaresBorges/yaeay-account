using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

public sealed record Query(Guid UserId) : IRequest<Result<Response>>;
