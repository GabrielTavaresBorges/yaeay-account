using MediatR;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.VerifyPasswordRecoveryCode;

public sealed class Handler(
    IUserRepository userRepository,
    IPasswordRecoveryChallengeRepository challengeRepository,
    IPasswordRecoveryCodeService codeService,
    IPasswordRecoveryPolicy policy,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(command.EmailAddress);
        if (emailResult.IsFailure)
            return Result<Response>.Failure(PasswordRecoveryChallengeErrors.InvalidOrExpired);

        var user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        var challenge = user is null ? null : await challengeRepository.GetOpenByUserIdAsync(user.Id, cancellationToken);
        var nowUtc = timeProvider.GetUtcNow();

        if (challenge is null || challenge.CodeHash is null || !challenge.IsCodeUsable(nowUtc))
            return Result<Response>.Failure(PasswordRecoveryChallengeErrors.InvalidOrExpired);

        if (!codeService.Matches(command.Code, challenge.CodeHash))
        {
            challenge.RegisterFailedAttempt(nowUtc, policy.MaximumFailedAttempts);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result<Response>.Failure(PasswordRecoveryChallengeErrors.InvalidOrExpired);
        }

        challenge.Verify(nowUtc, nowUtc + policy.ResetAuthorizationLifetime);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result<Response>.Success(new Response(challenge.Id));
    }
}
