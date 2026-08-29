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
        return (await connection.QueryAsync<UserSummary>(new CommandDefinition(sql, cancellationToken: cancellationToken))).AsList();
    }

    public async Task<IReadOnlyList<AuditEntry>> GetAuditAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "AdministratorId", "TargetUserId", "Action", "Justification", "OccurredAtUtc"
            FROM account_read."AdministrationAuditEntries" ORDER BY "OccurredAtUtc" DESC LIMIT 100;
            """;
        await using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<AuditEntry>(new CommandDefinition(sql, cancellationToken: cancellationToken))).AsList();
    }

    private sealed class Counts { public int TotalUsers { get; init; } public int PendingEmailConfirmation { get; init; } public int ActiveUsers { get; init; } public int SuspendedUsers { get; init; } public int DisabledUsers { get; init; } }
}
