using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.VerifyPasswordRecoveryCode;

public sealed record Command(string EmailAddress, string Code) : IRequest<Result<Response>>
{
    public override string ToString() => nameof(Command);
}
