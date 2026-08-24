using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.Repositories.PasswordRecoveryTemplates;

public interface IPasswordRecoveryTemplateRepository : IRepository<PasswordRecoveryTemplate>
{
    Task<PasswordRecoveryTemplate?> GetActiveAsync(PasswordRecoveryTemplatePurpose purpose, CancellationToken cancellationToken);
}
