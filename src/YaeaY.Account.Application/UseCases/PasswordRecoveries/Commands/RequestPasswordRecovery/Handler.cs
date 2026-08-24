using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.RequestPasswordRecovery;

public sealed class Handler(
    IUserRepository userRepository,
    IPasswordRecoveryChallengeRepository challengeRepository,
    IPasswordRecoveryPolicy policy,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<Handler> logger) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = Email.Create(command.EmailAddress);
            if (emailResult.IsFailure)
                return Result<Response>.Failure(emailResult.Error);

            var user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
            if (user is null || !user.EmailConfirmedAt.HasValue || user.Status is AccountStatus.PendingEmailConfirmation or AccountStatus.Disabled)
                return Result<Response>.Success(Response.Accepted);

            var nowUtc = timeProvider.GetUtcNow();
            var current = await challengeRepository.GetOpenByUserIdAsync(user.Id, cancellationToken);
            if (current is not null
                && ((current.IsAwaitingIssuance() && nowUtc - current.RequestedAt < policy.ResendInterval)
                    || current.IsCodeUsable(nowUtc)))
                return Result<Response>.Success(Response.Accepted);

            var mostRecentRequests = await challengeRepository.GetMostRecentRequestedAtAsync(
                user.Id,
                policy.MaximumRequestsPerWindow,
                cancellationToken);

            if (mostRecentRequests.Count == policy.MaximumRequestsPerWindow)
            {
                var newestRequest = mostRecentRequests[0];
                var oldestRequest = mostRecentRequests[^1];
                var limitWasReachedWithinWindow = newestRequest - oldestRequest <= policy.RequestWindow;
                var blockingPeriodIsActive = nowUtc - newestRequest < policy.RequestWindow;

                if (limitWasReachedWithinWindow && blockingPeriodIsActive)
                    return Result<Response>.Success(Response.Accepted);
            }

            if (current is not null)
                current.Invalidate(PasswordRecoveryChallengeInvalidationReason.Superseded, nowUtc);

            var challenge = PasswordRecoveryChallenge.Create(user.Id, user.Email, nowUtc);
            await challengeRepository.CreateAsync(challenge, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Result<Response>.Success(Response.Accepted);
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Password recovery request was rejected by a domain rule.");
            return Result<Response>.Success(Response.Accepted);
        }
    }
}
