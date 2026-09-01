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
        const string readModelSql = """
            SELECT "UserId", "Email", "FullName", "BirthDate", "Gender", "Status", "CreatedAt",
                   "EmailConfirmedAt", "FirstLoginAt", "LastLoginAt", "Phones", "Documents",
                   "LastEventOccurredOnUtc", "ProjectedAtUtc"
            FROM account_read."UserMyData" WHERE "UserId" = @UserId;
            """;

        const string latestWriteEventSql = """
            SELECT MAX("OccurredOnUtc")
            FROM account_write."OutboxMessages"
            WHERE "EventType" = 'YaeaY.Account.Domain.Events.Users.UserProfileChangedDomainEvent'
              AND "Payload" ->> 'UserId' = CAST(@UserId AS text);
            """;

        const string writeModelSql = """
            SELECT
                u."Id" AS "UserId", u."Email", u."UserName" AS "FullName", u."BirthDate", u."Gender", u."Status", u."CreatedAt",
                u."EmailConfirmedAt", u."FirstLoginAt", u."LastLoginAt",
                COALESCE((
                    SELECT jsonb_agg(jsonb_build_object(
                        'Id', p."Id", 'CallingCode', p."CallingCode", 'Country', p."RegionCode",
                        'AreaCode', p."AreaCode", 'Number', p."PhoneNumber", 'PhoneType', p."PhoneType",
                        'IsPrimary', p."IsPrimary", 'CreatedAt', p."CreatedAt") ORDER BY p."CreatedAt")
                    FROM account_write."UserPhones" p WHERE p."UserId" = u."Id"), '[]'::jsonb) AS "Phones",
                COALESCE((
                    SELECT jsonb_agg(jsonb_build_object(
                        'Id', d."Id", 'Type', d."DocumentType", 'Number', cpf."Number", 'CreatedAt', d."CreatedAt",
                        'Images', COALESCE((
                            SELECT jsonb_agg(jsonb_build_object(
                                'Id', i."Id", 'Position', i."Position", 'StorageObjectKey', i."StorageObjectKey", 'OriginalFileName', i."OriginalFileName",
                                'ContentType', i."ContentType", 'FileSizeBytes', i."FileSizeBytes", 'Sha256Hash', i."Sha256Hash", 'CreatedAt', i."CreatedAt") ORDER BY i."Position")
                            FROM account_write."UserDocumentImages" i WHERE i."UserDocumentId" = d."Id"), '[]'::jsonb)) ORDER BY d."CreatedAt")
                    FROM account_write."UserDocuments" d
                    LEFT JOIN account_write."UserDocumentCpf" cpf ON cpf."UserDocumentId" = d."Id"
                    WHERE d."UserId" = u."Id"), '[]'::jsonb) AS "Documents"
            FROM account_write."User" u
            WHERE u."Id" = @UserId;
            """;

        await using var connection = connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
            readModelSql, new { UserId = userId }, cancellationToken: cancellationToken));

        var latestWriteEventOccurredOnUtc = ReadModelFreshness.FromDatabaseTimestamp(
            await connection.QuerySingleOrDefaultAsync<DateTime?>(new CommandDefinition(
                latestWriteEventSql, new { UserId = userId }, cancellationToken: cancellationToken)));

        if (row is null || !ReadModelFreshness.IsCurrent(latestWriteEventOccurredOnUtc, row.LastEventOccurredOnUtc))
        {
            var writeRow = await connection.QuerySingleOrDefaultAsync<Row>(new CommandDefinition(
                writeModelSql, new { UserId = userId }, cancellationToken: cancellationToken));

            return writeRow is null ? null : ToResponse(writeRow, DateTimeOffset.UtcNow);
        }

        return ToResponse(row, row.ProjectedAtUtc);
    }

    private static Response ToResponse(Row row, DateTimeOffset projectedAtUtc) => new(
            row.UserId, row.Email, row.FullName, row.BirthDate, row.Gender, row.Status,
            row.CreatedAt, row.EmailConfirmedAt, row.FirstLoginAt, row.LastLoginAt,
            JsonSerializer.Deserialize<List<PhoneResponse>>(row.Phones, JsonOptions) ?? [],
            JsonSerializer.Deserialize<List<DocumentResponse>>(row.Documents, JsonOptions) ?? [],
            projectedAtUtc);

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
        public DateTimeOffset LastEventOccurredOnUtc { get; init; }
        public DateTimeOffset ProjectedAtUtc { get; init; }
    }
}
