using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;

namespace YaeaY.Account.Domain.Repositories.Administration;

public interface IAdministrationAuditRepository
{
    Task AddAsync(AdministrationAuditEntry entry, CancellationToken cancellationToken);
}
