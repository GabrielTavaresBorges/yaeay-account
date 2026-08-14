using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.EmailConfirmationTemplates;

public sealed class EmailConfirmationTemplateRepository : IEmailConfirmationTemplateRepository
{
    private readonly AppDbContext _context;

    public EmailConfirmationTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateEmailConfirmationTemplateAsync(
        EmailConfirmationTemplate emailConfirmationTemplate,
        CancellationToken cancellationToken)
    {
        await _context.EmailConfirmationTemplates.AddAsync(emailConfirmationTemplate, cancellationToken);
    }

    public async Task<EmailConfirmationTemplate?> GetActiveTemplateAsync(CancellationToken cancellationToken)
    {
        return await _context.EmailConfirmationTemplates
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
