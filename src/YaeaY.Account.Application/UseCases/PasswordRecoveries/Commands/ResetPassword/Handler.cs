using MediatR;
using YaeaY.Account.Application.Services.Identity.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.ResetPassword;

public sealed class Handler(
    IPasswordRecoveryChallengeRepository challengeRepository,
    IIdentityPasswordService identityPasswordService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var challenge = await challengeRepository.GetByIdAsync(command.ChallengeId, cancellationToken);
        var nowUtc = timeProvider.GetUtcNow();
        if (challenge is null || !challenge.IsResetAuthorized(nowUtc))
            return Result<Response>.Failure(PasswordRecoveryChallengeErrors.ResetNotAuthorized);

        var passwordResult = PasswordText.Create(command.NewPassword);
        if (passwordResult.IsFailure)
            return Result<Response>.Failure(passwordResult.Error);

        return await unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var identityResult = await identityPasswordService.ResetPasswordAsync(challenge.UserId, passwordResult.Value, transactionCancellationToken);
            if (identityResult.IsFailure)
                return Result<Response>.Failure(identityResult.Error);

            challenge.Consume(nowUtc);
            await unitOfWork.CommitAsync(transactionCancellationToken);
            return Result<Response>.Success(new Response(nowUtc));
        }, cancellationToken);
    }
}
