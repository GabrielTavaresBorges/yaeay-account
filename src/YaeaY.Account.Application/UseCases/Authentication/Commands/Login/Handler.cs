using MediatR;
using Microsoft.Extensions.Logging;
using YaeaY.Account.Application.Services.Identity.Errors;
using YaeaY.Account.Application.Services.Identity.Interfaces;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Application.UseCases.Authentication.Commands.Login;

public sealed class Handler(
    IUserRepository userRepository,
    IIdentityAccountService identityAccountService,
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
            if (user is null)
                return Result<Response>.Failure(IdentityErrors.InvalidCredentials);

            var credentialResult = await identityAccountService.ValidateCredentialsAsync(
                user.Id,
                command.Password,
                cancellationToken);

            if (credentialResult.IsFailure)
                return Result<Response>.Failure(credentialResult.Error);

            var accountStateError = GetAccountStateError(user.Status, user.EmailConfirmedAt);
            if (accountStateError is not null)
                return Result<Response>.Failure(accountStateError);

            var nowUtc = timeProvider.GetUtcNow();
            user.RegisterSuccessfulLogin(nowUtc);
            await unitOfWork.CommitAsync(cancellationToken);

            var signInResult = await identityAccountService.SignInAsync(
                user.Id,
                command.RememberMe,
                cancellationToken);

            if (signInResult.IsFailure)
                return Result<Response>.Failure(signInResult.Error);

            return Result<Response>.Success(new Response(user.Id, user.FullName.Name, nowUtc));
        }
        catch (DomainException exception)
        {
            logger.LogWarning(exception, "Login was rejected by a domain rule.");
            return Result<Response>.Failure(exception.Error);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while authenticating the account.");
            return Result<Response>.Failure(new Error(
                "authentication.unexpected-error",
                "An unexpected error occurred while authenticating the account.",
                ErrorCategory.Unexpected,
                ErrorRule.Unexpected));
        }
    }

    private static Error? GetAccountStateError(
        AccountStatus status,
        DateTimeOffset? emailConfirmedAt)
    {
        if (!emailConfirmedAt.HasValue || status == AccountStatus.PendingEmailConfirmation)
            return UserErrors.EmailConfirmationRequiredForLogin;

        return status switch
        {
            AccountStatus.Active => null,
            AccountStatus.Suspended => UserErrors.SuspendedAccountCannotLogin,
            AccountStatus.Disabled => UserErrors.DisabledAccountCannotLogin,
            _ => UserErrors.AccountCannotLogin
        };
    }
}
