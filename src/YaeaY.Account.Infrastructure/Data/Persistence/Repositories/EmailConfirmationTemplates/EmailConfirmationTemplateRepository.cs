using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;
using YaeaY.Account.Domain.Repositories.EmailConfirmationSettings;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.EmailConfirmationSettings;

public sealed class EmailConfirmationSettingRepository : IEmailConfirmationSettingRepository
{
    private readonly AppDbContext _context;

    public EmailConfirmationSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateEmailConfirmationSettingsAsync(
        EmailConfirmationSetting emailConfirmationSetting,
        CancellationToken cancellationToken)
    {
        await _context.EmailConfirmationSettings.AddAsync(emailConfirmationSetting, cancellationToken);
    }

    public async Task<EmailConfirmationSetting?> GetSettingsActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.EmailConfirmationSettings
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
