using MediatR;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.IssuePasswordRecoveryCode;

public sealed class Handler(
    IPasswordRecoveryChallengeRepository challengeRepository,
    IUserRepository userRepository,
    IPasswordRecoveryCodeService codeService,
    IPasswordRecoveryPolicy policy,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<Command, Result<Response>>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        var challenge = await challengeRepository.GetByIdAsync(command.ChallengeId, cancellationToken);
        if (challenge is null)
            return Result<Response>.Failure(PasswordRecoveryChallengeErrors.InvalidOrExpired);

        var user = await userRepository.GetByIdAsync(challenge.UserId, cancellationToken);
        if (user is null)
            return Result<Response>.Failure(UserErrors.NotFound);

        if (!challenge.IsAwaitingIssuance())
            return Result<Response>.Success(new Response(challenge.Id, user.Email.EmailAddress, user.FullName.Name, null, false));

        var generated = codeService.Generate();
        var nowUtc = timeProvider.GetUtcNow();
        challenge.Issue(generated.CodeHash, nowUtc, nowUtc + policy.CodeLifetime);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Response>.Success(new Response(challenge.Id, user.Email.EmailAddress, user.FullName.Name, generated.RevealRawCode(), true));
    }
}
