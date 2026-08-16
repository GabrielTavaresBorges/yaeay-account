using MediatR;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Policies.EmailConfirmations;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

public sealed class Handler : IRequestHandler<Command, Result<Response>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailConfirmationTokenRepository _tokenRepository;
    private readonly IEmailConfirmationTokenService _tokenService;
    private readonly IEmailConfirmationTokenExpirationPolicy _expirationPolicy;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public Handler(
        IUserRepository userRepository,
        IEmailConfirmationTokenRepository tokenRepository,
        IEmailConfirmationTokenService tokenService,
        IEmailConfirmationTokenExpirationPolicy expirationPolicy,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _tokenService = tokenService;
        _expirationPolicy = expirationPolicy;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.DomainEvent);

        var user = await _userRepository.GetByIdAsync(command.DomainEvent.UserId, cancellationToken);

        if (user is null)
            return Result<Response>.Failure(UserErrors.NotFound);

        if (user.Status != AccountStatus.PendingEmailConfirmation)
            return Result<Response>.Failure(EmailConfirmationTokenErrors.AccountNotPendingEmailConfirmation);

        if (await _tokenRepository.HasPendingTokenAsync(user.Id, cancellationToken))
            return Result<Response>.Failure(EmailConfirmationTokenErrors.PendingTokenAlreadyExists);

        var expiresAt = _expirationPolicy.GetInitialStageExpiration(user.CreatedAt);

        if (expiresAt <= _timeProvider.GetUtcNow())
            return Result<Response>.Failure(EmailConfirmationTokenErrors.InitialStageExpired);

        var generatedToken = await _tokenService.GenerateTokenAsync();

        var confirmationToken = EmailConfirmationToken.Create(
            userId: user.Id,
            email: user.Email,
            tokenHash: generatedToken.GetTokenHash(),
            expiresAt: expiresAt,
            requestedBy: EmailConfirmationTokenRequestedBy.System,
            requestReason: EmailConfirmationTokenRequestReason.AccountCreated);

        await _tokenRepository.CreateEmailConfirmationTokenAsync(confirmationToken, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<Response>.Success(
            new Response(
                tokenId: confirmationToken.Id,
                toEmail: user.Email.EmailAddress,
                fullName: user.FullName.Name,
                rawToken: generatedToken.RevealRawToken(),
                expiresAt: confirmationToken.ExpiresAt));
    }
}
