using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.IssuePasswordRecoveryCode;

public sealed record Command(Guid ChallengeId) : IRequest<Result<Response>>;
