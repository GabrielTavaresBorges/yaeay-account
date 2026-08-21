using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;

namespace YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;

public interface IPasswordRecoveryChallengeRepository : IRepository<PasswordRecoveryChallenge>
{
    Task CreateAsync(PasswordRecoveryChallenge challenge, CancellationToken cancellationToken);
    Task<PasswordRecoveryChallenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PasswordRecoveryChallenge?> GetOpenByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DateTimeOffset>> GetMostRecentRequestedAtAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken);
}
