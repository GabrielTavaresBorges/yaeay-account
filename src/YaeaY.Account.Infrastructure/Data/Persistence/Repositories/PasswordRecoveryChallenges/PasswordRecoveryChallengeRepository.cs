using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryChallenges;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.PasswordRecoveryChallenges;

public sealed class PasswordRecoveryChallengeRepository(AppDbContext context) : IPasswordRecoveryChallengeRepository
{
    public async Task CreateAsync(PasswordRecoveryChallenge challenge, CancellationToken cancellationToken) =>
        await context.PasswordRecoveryChallenges.AddAsync(challenge, cancellationToken);

    public Task<PasswordRecoveryChallenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.PasswordRecoveryChallenges.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<PasswordRecoveryChallenge?> GetOpenByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        context.PasswordRecoveryChallenges.SingleOrDefaultAsync(item =>
            item.UserId == userId && item.ConsumedAt == null && item.InvalidatedAt == null, cancellationToken);

    public async Task<IReadOnlyList<DateTimeOffset>> GetMostRecentRequestedAtAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken) =>
        await context.PasswordRecoveryChallenges
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.RequestedAt)
            .Select(item => item.RequestedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
