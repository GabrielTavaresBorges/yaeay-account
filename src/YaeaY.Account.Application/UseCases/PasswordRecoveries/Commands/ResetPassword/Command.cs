using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.ResetPassword;

public sealed record Command(Guid ChallengeId, string NewPassword, string ConfirmPassword) : IRequest<Result<Response>>
{
    public override string ToString() => nameof(Command);
}
