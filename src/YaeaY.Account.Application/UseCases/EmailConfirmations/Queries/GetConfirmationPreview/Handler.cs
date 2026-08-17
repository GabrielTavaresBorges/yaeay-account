using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.EmailConfirmationTokens;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Domain.Repositories.Users;

namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Queries.GetConfirmationPreview;

public sealed class Handler : IRequestHandler<Query, Result<Response>>
{
    private readonly IEmailConfirmationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailConfirmationTokenService _tokenService;
    private readonly EmailAddressMasker _emailAddressMasker;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<Handler> _logger;

    public Handler(
        IEmailConfirmationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IEmailConfirmationTokenService tokenService,
        EmailAddressMasker emailAddressMasker,
        TimeProvider timeProvider,
        ILogger<Handler> logger)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailAddressMasker = emailAddressMasker;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Result<Response>> Handle(
        Query query,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(query);

            var tokenHashResult = _tokenService.HashToken(query.Token);
            if (tokenHashResult.IsFailure)
                return Result<Response>.Failure(tokenHashResult.Error);

            var confirmationToken = await _tokenRepository.GetByHashAsync(
                tokenHashResult.Value,
                cancellationToken);

            if (confirmationToken is null)
                return Result<Response>.Failure(EmailConfirmationTokenErrors.NotFound);

            var nowUtc = _timeProvider.GetUtcNow();

            if (confirmationToken.IsUsed())
                return Result<Response>.Failure(UserErrors.EmailAlreadyConfirmed);

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

            return Result<Response>.Success(
                new Response(
                    MaskedEmail: _emailAddressMasker.Mask(user.Email)));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected error while preparing an email confirmation preview.");

            return Result<Response>.Failure(
                new Error(
                    Code: "email-confirmation-preview.unexpected-error",
                    Message: "An unexpected error occurred while preparing the email confirmation preview.",
                    Category: ErrorCategory.Unexpected,
                    Rule: ErrorRule.Unexpected));
        }
    }
}
