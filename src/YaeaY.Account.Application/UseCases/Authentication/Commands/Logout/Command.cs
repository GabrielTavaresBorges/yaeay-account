using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Logout;

public sealed record Command : IRequest<Result<Response>>;

public sealed record Response(bool SignedOut);
