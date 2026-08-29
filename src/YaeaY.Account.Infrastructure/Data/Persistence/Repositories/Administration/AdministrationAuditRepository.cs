using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;
using YaeaY.Account.Domain.Repositories.Administration;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.Administration;

public sealed class AdministrationAuditRepository(AppDbContext context) : IAdministrationAuditRepository
{
    public Task AddAsync(AdministrationAuditEntry entry, CancellationToken cancellationToken)
        => context.Set<AdministrationAuditEntry>().AddAsync(entry, cancellationToken).AsTask();
}
