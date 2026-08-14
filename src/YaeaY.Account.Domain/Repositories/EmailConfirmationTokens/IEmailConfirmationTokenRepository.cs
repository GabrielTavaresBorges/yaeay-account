using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;

namespace YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;

public interface IEmailConfirmationTokenRepository : IRepository<EmailConfirmationToken>
{
    Task<bool> HasPendingTokenAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task CreateEmailConfirmationTokenAsync(
        EmailConfirmationToken emailConfirmationToken,
        CancellationToken cancellationToken);
}

