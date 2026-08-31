namespace YaeaY.Account.Application.Services.Administration.Interfaces;

public interface IAdministrationConfigurationService
{
    Task<EmailConfirmationTemplateSettings?> GetEmailConfirmationTemplateAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<IdentityRoleSummary>> GetRolesAsync(CancellationToken cancellationToken);
    Task<RoleCreationResult> CreateRoleAsync(string name, CancellationToken cancellationToken);
}

public sealed record EmailConfirmationTemplateSettings(Guid Id, string Subject, string BodyHtml, DateTimeOffset UpdatedAt);
public sealed record IdentityRoleSummary(Guid Id, string Name);
public sealed record RoleCreationResult(bool Succeeded, string? Error);
