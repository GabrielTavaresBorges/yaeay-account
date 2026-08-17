using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.DomainUsers.AddAsync(user, cancellationToken);
    }

    public Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        _context.DomainUsers.Update(user);
        return Task.CompletedTask;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return _context.DomainUsers.FirstOrDefaultAsync(
            user => user.Email.EmailAddress == email.EmailAddress,
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return _context.DomainUsers.AnyAsync(
            user => user.Email.EmailAddress == email.EmailAddress, cancellationToken);
    }
}
