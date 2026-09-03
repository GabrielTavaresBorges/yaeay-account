using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Dates;
using YaeaY.Account.Domain.ValueObjects.Names;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateBasicData;

public sealed class Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<Handler> logger)
    : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
            if (user is null) return Result<Response>.Failure(UserErrors.NotFound);

            var changed = false;
            if (command.FullName is not null && !string.Equals(user.FullName.Name, command.FullName.Trim(), StringComparison.Ordinal))
            {
                var result = FullName.Create(command.FullName);
                if (result.IsFailure) return Result<Response>.Failure(result.Error);
                user.ChangeFullName(result.Value);
                changed = true;
            }
            if (command.BirthDate.HasValue && user.BirthDate.Date != command.BirthDate.Value)
            {
                var result = BirthDate.Create(command.BirthDate.Value);
                if (result.IsFailure) return Result<Response>.Failure(result.Error);
                user.ChangeBirthDate(result.Value);
                changed = true;
            }
            if (command.Gender.HasValue && user.Gender != command.Gender.Value)
            {
                user.ChangeGender(command.Gender.Value);
                changed = true;
            }
            if (!changed)
                return Result<Response>.Success(new Response(user.Id, user.FullName.Name, user.BirthDate.Date, user.Gender, false, "No changes to apply."));

            await userRepository.UpdateUserAsync(user, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<Response>.Success(new Response(user.Id, user.FullName.Name, user.BirthDate.Date, user.Gender, true, "Basic data updated successfully."));
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Domain error updating basic data for user {UserId}.", command.Id);
            return Result<Response>.Failure(exception.Error);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error updating basic data for user {UserId}.", command.Id);
            return Result<Response>.Failure(new Error("unexpected.error", "An unexpected error occurred.", ErrorCategory.Unexpected, ErrorRule.Unexpected));
        }
    }
}
