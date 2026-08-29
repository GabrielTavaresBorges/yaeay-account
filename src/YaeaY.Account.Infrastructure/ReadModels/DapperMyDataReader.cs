using System.Text.Json;
using Dapper;
using YaeaY.Account.Application.Services.ReadModels.Interfaces;
using YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

namespace YaeaY.Account.Infrastructure.ReadModels;

public sealed class DapperMyDataReader(ReadModelConnectionFactory connectionFactory) : IMyDataReader
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "UserId", "Email", "FullName", "BirthDate", "Gender", "Status", "CreatedAt",
                   "EmailConfirmedAt", "FirstLoginAt", "LastLoginAt", "Phones", "Documents", "ProjectedAtUtc"
            FROM account_read."UserMyData" WHERE "UserId" = @UserId;
            """;

        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
            sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return row is null ? null : new Response(
            row.UserId, row.Email, row.FullName, row.BirthDate, row.Gender, row.Status,
            row.CreatedAt, row.EmailConfirmedAt, row.FirstLoginAt, row.LastLoginAt,
            JsonSerializer.Deserialize<List<PhoneResponse>>(row.Phones, JsonOptions) ?? [],
            JsonSerializer.Deserialize<List<DocumentResponse>>(row.Documents, JsonOptions) ?? [],
            row.ProjectedAtUtc);
    }

    private sealed class Row
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public DateOnly BirthDate { get; init; }
        public string Gender { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? EmailConfirmedAt { get; init; }
        public DateTimeOffset? FirstLoginAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
        public string Phones { get; init; } = "[]";
        public string Documents { get; init; } = "[]";
        public DateTimeOffset ProjectedAtUtc { get; init; }
    }
}
