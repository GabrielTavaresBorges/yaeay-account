using MediatR;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Administration;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.Administration.Commands.ChangeUserStatus;

public sealed class Handler(IUserRepository users, IAdministrationAuditRepository audit, IUnitOfWork unitOfWork, TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null) return Result<Response>.Failure(UserErrors.NotFound);
        try
        {
            var now = timeProvider.GetUtcNow();
            switch (command.Status)
            {
                case AccountStatus.Suspended:
                    user.Suspend(command.SuspensionReason ?? SuspensionReason.Unknown, command.Justification, now, command.SuspendedUntilUtc);
                    break;
                case AccountStatus.Disabled:
                    user.Disable(command.Justification);
                    break;
                case AccountStatus.Active:
                    user.Reactivate(command.Justification);
                    break;
                default:
                    return Result<Response>.Failure(UserErrors.AccountCannotLogin);
            }
            await audit.AddAsync(AdministrationAuditEntry.Create(command.AdministratorId, user.Id, $"User.{command.Status}", command.Justification, now), cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<Response>.Success(new Response(user.Id, user.Status.ToString()));
        }
        catch (DomainException exception) { return Result<Response>.Failure(exception.Error); }
    }
}
