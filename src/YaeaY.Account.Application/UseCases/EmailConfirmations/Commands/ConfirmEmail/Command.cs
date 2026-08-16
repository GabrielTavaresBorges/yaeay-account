using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;

public sealed record Command(string Token) : IRequest<Result<Response>>;
