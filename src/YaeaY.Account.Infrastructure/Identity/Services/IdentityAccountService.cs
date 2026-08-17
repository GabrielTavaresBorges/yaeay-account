using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using YaeaY.Account.Application.Services.Identity.Errors;
using YaeaY.Account.Application.Services.Identity.Interfaces;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Securities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.Users;
using YaeaY.Account.Infrastructure.Identity.Models;
using YaeaY.Account.Infrastructure.Identity.Configurations;
using YaeaY.Account.Infrastructure.Identity.Constants;

namespace YaeaY.Account.Infrastructure.Identity.Services;

public sealed class IdentityAccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptions<AccountSessionOptions> sessionOptions,
    TimeProvider timeProvider,
    ILogger<IdentityAccountService> logger) : IIdentityAccountService
{
    public async Task<Result<IdentityOperation>> CreateAsync(
        Guid userId,
        Email email,
        PasswordText password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = new ApplicationUser
        {
            Id = userId,
            UserName = email.EmailAddress,
            Email = email.EmailAddress,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        IdentityResult result;

        try
        {
            result = await userManager.CreateAsync(identityUser, password.Password);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            throw new DomainException(UserErrors.EmailAlreadyInUse, exception);
        }
        if (result.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(identityUser, Roles.User);
            if (roleResult.Succeeded)
                return Result<IdentityOperation>.Success(IdentityOperation.Success);

            logger.LogError(
                "Default identity role assignment failed for user {UserId}. Codes: {Codes}",
                userId,
                string.Join(',', roleResult.Errors.Select(error => error.Code)));

            throw new DomainException(IdentityErrors.CreationFailed);
        }

        logger.LogWarning(
            "Identity credential creation failed for user {UserId}. Codes: {Codes}",
            userId,
            string.Join(',', result.Errors.Select(error => error.Code)));

        return Result<IdentityOperation>.Failure(IdentityErrors.CreationFailed);
    }

    public async Task<Result<IdentityOperation>> ConfirmEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null)
            return Result<IdentityOperation>.Failure(IdentityErrors.NotFound);

        if (identityUser.EmailConfirmed)
            return Result<IdentityOperation>.Success(IdentityOperation.Success);

        identityUser.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(identityUser);

        if (result.Succeeded)
            return Result<IdentityOperation>.Success(IdentityOperation.Success);

        logger.LogError(
            "Identity email confirmation failed for user {UserId}. Codes: {Codes}",
            userId,
            string.Join(',', result.Errors.Select(error => error.Code)));

        return Result<IdentityOperation>.Failure(IdentityErrors.EmailConfirmationFailed);
    }

    public async Task<Result<IdentityOperation>> ValidateCredentialsAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null)
            return Result<IdentityOperation>.Failure(IdentityErrors.InvalidCredentials);

        if (await userManager.IsLockedOutAsync(identityUser))
            return Result<IdentityOperation>.Failure(IdentityErrors.LockedOut);

        if (!await userManager.CheckPasswordAsync(identityUser, password))
        {
            await userManager.AccessFailedAsync(identityUser);

            return await userManager.IsLockedOutAsync(identityUser)
                ? Result<IdentityOperation>.Failure(IdentityErrors.LockedOut)
                : Result<IdentityOperation>.Failure(IdentityErrors.InvalidCredentials);
        }

        if (identityUser.AccessFailedCount > 0)
            await userManager.ResetAccessFailedCountAsync(identityUser);

        return Result<IdentityOperation>.Success(IdentityOperation.Success);
    }

    public async Task<Result<IdentityOperation>> SignInAsync(
        Guid userId,
        bool isPersistent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var identityUser = await userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null)
            return Result<IdentityOperation>.Failure(IdentityErrors.SignInFailed);

        var authenticationProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = isPersistent,
            AllowRefresh = true,
            ExpiresUtc = isPersistent
                ? timeProvider.GetUtcNow().AddDays(sessionOptions.Value.RememberMeDurationInDays)
                : null
        };

        await signInManager.SignInAsync(identityUser, authenticationProperties);
        return Result<IdentityOperation>.Success(IdentityOperation.Success);
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return signInManager.SignOutAsync();
    }
}
