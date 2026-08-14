using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;

namespace YaeaY.Account.Domain.Repositories.EmailConfirmationSettings;

public interface IEmailConfirmationSettingRepository : IRepository<EmailConfirmationSetting>
{
    Task CreateEmailConfirmationSettingsAsync(
        EmailConfirmationSetting emailConfirmationSettings,
        CancellationToken cancellationToken);

    Task<EmailConfirmationSetting?> GetSettingsActiveAsync(CancellationToken cancellationToken);
}