namespace YaeaY.Account.Application.Services.Administration.Interfaces;

public interface IAdministrationReader
{
    Task<Overview> GetOverviewAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditEntry>> GetAuditAsync(CancellationToken cancellationToken);
}

public sealed record Overview(int TotalUsers, int PendingEmailConfirmation, int ActiveUsers, int SuspendedUsers, int DisabledUsers, int PendingOutboxMessages);
public sealed record UserSummary(Guid UserId, string Email, string FullName, string Status, DateTimeOffset CreatedAt, DateTimeOffset? EmailConfirmedAt, DateTimeOffset? LastLoginAt);
public sealed record AuditEntry(Guid Id, Guid AdministratorId, Guid? TargetUserId, string Action, string Justification, DateTimeOffset OccurredAtUtc);
