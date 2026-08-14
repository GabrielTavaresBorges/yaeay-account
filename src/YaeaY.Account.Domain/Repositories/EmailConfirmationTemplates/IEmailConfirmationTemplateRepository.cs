using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;

namespace YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;

public interface IEmailConfirmationTemplateRepository : IRepository<EmailConfirmationTemplate>
{
    Task CreateEmailConfirmationTemplateAsync(
        EmailConfirmationTemplate emailConfirmationTemplate,
        CancellationToken cancellationToken);

    Task<EmailConfirmationTemplate?> GetActiveTemplateAsync(CancellationToken cancellationToken);
}
