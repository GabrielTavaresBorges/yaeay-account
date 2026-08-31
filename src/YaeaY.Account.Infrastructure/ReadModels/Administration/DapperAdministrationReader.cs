using Dapper;
using YaeaY.Account.Application.Services.Administration.Interfaces;

namespace YaeaY.Account.Infrastructure.ReadModels.Administration;

public sealed class DapperAdministrationReader(ReadModelConnectionFactory connectionFactory) : IAdministrationReader
{
    public async Task<Overview> GetOverviewAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)::int AS "TotalUsers",
                   COUNT(*) FILTER (WHERE "Status" = 'PendingEmailConfirmation')::int AS "PendingEmailConfirmation",
                   COUNT(*) FILTER (WHERE "Status" = 'Active')::int AS "ActiveUsers",
                   COUNT(*) FILTER (WHERE "Status" = 'Suspended')::int AS "SuspendedUsers",
                   COUNT(*) FILTER (WHERE "Status" = 'Disabled')::int AS "DisabledUsers"
            FROM account_read."UserMyData";
            """;
        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleAsync<Counts>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        const string outboxSql = "SELECT COUNT(*)::int FROM account_read.\"AdministrationOutbox\" WHERE \"ProcessedOnUtc\" IS NULL;";
        var pending = await connection.QuerySingleAsync<int>(new CommandDefinition(outboxSql, cancellationToken: cancellationToken));
        return new Overview(row.TotalUsers, row.PendingEmailConfirmation, row.ActiveUsers, row.SuspendedUsers, row.DisabledUsers, pending);
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "UserId", "Email", "FullName", "Status", "CreatedAt", "EmailConfirmedAt", "LastLoginAt"
            FROM account_read."UserMyData" ORDER BY "CreatedAt" DESC;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<UserSummaryRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(row => new UserSummary(
            row.UserId,
            row.Email,
            row.FullName,
            row.Status,
            row.CreatedAt,
            row.EmailConfirmedAt,
            row.LastLoginAt)).ToList();
    }

    public async Task<IReadOnlyList<AuditEntry>> GetAuditAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "AdministratorId", "TargetUserId", "Action", "Justification", "OccurredAtUtc"
            FROM account_read."AdministrationAuditEntries" ORDER BY "OccurredAtUtc" DESC LIMIT 100;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<AuditEntryRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(row => new AuditEntry(
            row.Id,
            row.AdministratorId,
            row.TargetUserId,
            row.Action,
            row.Justification,
            row.OccurredAtUtc)).ToList();
    }

    private sealed class Counts { public int TotalUsers { get; init; } public int PendingEmailConfirmation { get; init; } public int ActiveUsers { get; init; } public int SuspendedUsers { get; init; } public int DisabledUsers { get; init; } }

    private sealed class UserSummaryRow
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? EmailConfirmedAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
    }

    private sealed class AuditEntryRow
    {
        public Guid Id { get; init; }
        public Guid AdministratorId { get; init; }
        public Guid? TargetUserId { get; init; }
        public string Action { get; init; } = string.Empty;
        public string Justification { get; init; } = string.Empty;
        public DateTimeOffset OccurredAtUtc { get; init; }
    }
}
