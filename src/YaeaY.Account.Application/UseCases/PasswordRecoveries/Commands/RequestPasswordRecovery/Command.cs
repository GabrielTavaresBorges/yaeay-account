using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;

public sealed record Command(string EmailAddress) : IRequest<Result<Response>>;
