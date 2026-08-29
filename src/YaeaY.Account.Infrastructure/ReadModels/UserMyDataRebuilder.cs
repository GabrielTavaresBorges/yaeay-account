using Dapper;

namespace YaeaY.Account.Infrastructure.ReadModels;

public sealed class UserMyDataRebuilder(
    ReadModelConnectionFactory connectionFactory,
    UserMyDataProjector projector,
    TimeProvider timeProvider)
{
    public async Task<int> RebuildAllAsync(CancellationToken cancellationToken)
    {
        const string userIdsSql = """
            SELECT "Id"
            FROM account_write."User"
            ORDER BY "Id";
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var userIds = (await connection.QueryAsync<Guid>(new CommandDefinition(
            userIdsSql,
            cancellationToken: cancellationToken))).AsList();

        var snapshotAtUtc = timeProvider.GetUtcNow();
        foreach (var userId in userIds)
            await projector.RebuildAsync(userId, snapshotAtUtc, cancellationToken);

        return userIds.Count;
    }
}
