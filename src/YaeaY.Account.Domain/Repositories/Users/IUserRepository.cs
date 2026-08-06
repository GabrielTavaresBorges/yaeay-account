using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;

namespace YaeaY.Account.Domain.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);
    Task CreateUserAsync(User user, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}