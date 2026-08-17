using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;

public sealed class Command : IRequest<Result<Response>>
{
    public string Token { get; }

    public Command(string token)
    {
        Token = token;
    }

    public override string ToString() => nameof(Command);
}
