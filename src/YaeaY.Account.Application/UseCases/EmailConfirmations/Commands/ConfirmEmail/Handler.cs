using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.ConfirmEmail;

public sealed class Handler : IRequestHandler<Command, Result<Response>>
{
    private readonly IEmailConfirmationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailConfirmationTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<Handler> _logger;

    public Handler(
        IEmailConfirmationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IEmailConfirmationTokenService tokenService,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<Response>> Handle(
        Command command,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(command);

            var tokenHashResult = _tokenService.HashToken(command.Token);
            if (tokenHashResult.IsFailure)
                return Result<Response>.Failure(tokenHashResult.Error);

            var confirmationToken = await _tokenRepository.GetByHashAsync(
                tokenHashResult.Value,
                cancellationToken);

            if (confirmationToken is null)
                return Result<Response>.Failure(EmailConfirmationTokenErrors.NotFound);

            var nowUtc = _timeProvider.GetUtcNow();

            if (confirmationToken.IsUsed())
                return Result<Response>.Failure(EmailConfirmationTokenErrors.AlreadyUsed);

            if (confirmationToken.IsInvalidated())
                return Result<Response>.Failure(EmailConfirmationTokenErrors.Invalidated);

            if (confirmationToken.IsExpired(nowUtc))
                return Result<Response>.Failure(EmailConfirmationTokenErrors.Expired);

            var user = await _userRepository.GetByIdAsync(
                confirmationToken.UserId,
                cancellationToken);

            if (user is null)
                return Result<Response>.Failure(UserErrors.NotFound);

            if (!string.Equals(
                    user.Email.EmailAddress,
                    confirmationToken.Email.EmailAddress,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Result<Response>.Failure(
                    EmailConfirmationTokenErrors.EmailDoesNotMatchAccount);
            }

            user.ConfirmEmail(nowUtc);
            confirmationToken.MarkAsUsed(nowUtc);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<Response>.Success(
                new Response(
                    UserId: user.Id,
                    Status: user.Status,
                    EmailConfirmedAt: user.EmailConfirmedAt!.Value));
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(exception, "Email confirmation was rejected by a domain rule.");
            return Result<Response>.Failure(exception.Error);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while confirming an email address.");

            return Result<Response>.Failure(
                new Error(
                    Code: "email-confirmation.unexpected-error",
                    Message: "An unexpected error occurred while confirming the email address.",
                    Category: ErrorCategory.Unexpected,
                    Rule: ErrorRule.Unexpected));
        }
    }
}
