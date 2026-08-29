using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Administration.Commands.ChangeUserStatus;

public sealed record Command(Guid AdministratorId, Guid UserId, AccountStatus Status, SuspensionReason? SuspensionReason, DateTimeOffset? SuspendedUntilUtc, string Justification) : IRequest<Result<Response>>;
