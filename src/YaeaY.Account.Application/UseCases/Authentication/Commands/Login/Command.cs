using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Login;

public sealed record Command(
    string EmailAddress,
    string Password,
    bool RememberMe) : IRequest<Result<Response>>;
