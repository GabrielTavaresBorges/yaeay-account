using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.EmailConfirmationTokens;

public sealed class EmailConfirmationTokenRepository : IEmailConfirmationTokenRepository
{
    private readonly AppDbContext _context;

    public EmailConfirmationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasPendingTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.EmailConfirmationTokens.AnyAsync(
            token => token.UserId == userId &&
                     token.UsedAt == null &&
                     token.InvalidatedAt == null,
            cancellationToken);
    }

    public async Task CreateEmailConfirmationTokenAsync(EmailConfirmationToken emailConfirmationToken, CancellationToken cancellationToken)
    {
        await _context.EmailConfirmationTokens.AddAsync(emailConfirmationToken, cancellationToken);
    }
}
