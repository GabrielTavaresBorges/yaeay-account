using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.Services.Identity.Interfaces;

public interface IIdentityAccountService
{
    Task<Result<IdentityOperation>> CreateAsync(
        Guid userId,
        Email email,
        PasswordText password,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityOperation>> ConfirmEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityOperation>> ValidateCredentialsAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityOperation>> SignInAsync(
        Guid userId,
        bool isPersistent,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}

public sealed record IdentityOperation(bool Completed)
{
    public static readonly IdentityOperation Success = new(true);
}
