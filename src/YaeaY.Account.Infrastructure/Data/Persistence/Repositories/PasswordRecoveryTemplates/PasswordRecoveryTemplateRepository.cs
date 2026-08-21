using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Repositories.PasswordRecoveryTemplates;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.PasswordRecoveryTemplates;

public sealed class PasswordRecoveryTemplateRepository(AppDbContext context) : IPasswordRecoveryTemplateRepository
{
    public Task<PasswordRecoveryTemplate?> GetActiveAsync(PasswordRecoveryTemplatePurpose purpose, CancellationToken cancellationToken) =>
        context.PasswordRecoveryTemplates.SingleOrDefaultAsync(item => item.Purpose == purpose && item.IsActive, cancellationToken);
}
