namespace YaeaY.Account.Domain.Abstraction.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
