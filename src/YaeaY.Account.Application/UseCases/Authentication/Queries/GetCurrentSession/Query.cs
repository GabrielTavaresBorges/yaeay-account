using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Authentication.Queries.GetCurrentSession;

public sealed record Query(Guid UserId, bool CanManageAccount = false) : IRequest<Result<Response>>;
