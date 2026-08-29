using Dapper;

namespace YaeaY.Account.Infrastructure.ReadModels.Administration;

public sealed class AdministrationReadModelRebuilder(ReadModelConnectionFactory connectionFactory)
{
    public async Task RebuildAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            TRUNCATE TABLE account_read."AdministrationAuditEntries", account_read."AdministrationOutbox";
            INSERT INTO account_read."AdministrationAuditEntries" ("Id", "AdministratorId", "TargetUserId", "Action", "Justification", "OccurredAtUtc", "ProjectedAtUtc")
            SELECT "Id", "AdministratorId", "TargetUserId", "Action", "Justification", "OccurredAtUtc", CURRENT_TIMESTAMP
            FROM account_write."AdministrationAuditEntries";
            INSERT INTO account_read."AdministrationOutbox" ("Id", "ProcessedOnUtc", "ProjectedAtUtc")
            SELECT "Id", "ProcessedOnUtc", CURRENT_TIMESTAMP FROM account_write."OutboxMessages";
            """;
        await using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
