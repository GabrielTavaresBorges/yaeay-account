using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Application.Services.Administration.Interfaces;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Identity.Models;

namespace YaeaY.Account.Infrastructure.Identity.Services;

public sealed class AdministrationConfigurationService(
    IEmailConfirmationTemplateRepository templates,
    AppDbContext context,
    RoleManager<ApplicationRole> roleManager) : IAdministrationConfigurationService
{
    public async Task<EmailConfirmationTemplateSettings?> GetEmailConfirmationTemplateAsync(CancellationToken cancellationToken)
    {
        var template = await templates.GetActiveTemplateAsync(cancellationToken);
        return template is null ? null : new(template.Id, template.Subject, template.BodyHtml, template.UpdatedAt);
    }

    public async Task<IReadOnlyList<IdentityRoleSummary>> GetRolesAsync(CancellationToken cancellationToken) =>
        await context.Roles.OrderBy(role => role.Name).Select(role => new IdentityRoleSummary(role.Id, role.Name!)).ToListAsync(cancellationToken);

    public async Task<RoleCreationResult> CreateRoleAsync(string name, CancellationToken cancellationToken)
    {
        var result = await roleManager.CreateAsync(new ApplicationRole { Id = Guid.NewGuid(), Name = name.Trim() });
        return result.Succeeded ? new(true, null) : new(false, string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
