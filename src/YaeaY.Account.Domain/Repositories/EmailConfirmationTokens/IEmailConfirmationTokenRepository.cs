using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;

using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;

public interface IEmailConfirmationTokenRepository : IRepository<EmailConfirmationToken>
{
    Task<EmailConfirmationToken?> GetByHashAsync(
        TokenHash tokenHash,
        CancellationToken cancellationToken);

    Task<bool> HasPendingTokenAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task CreateEmailConfirmationTokenAsync(
        EmailConfirmationToken emailConfirmationToken,
        CancellationToken cancellationToken);
}

