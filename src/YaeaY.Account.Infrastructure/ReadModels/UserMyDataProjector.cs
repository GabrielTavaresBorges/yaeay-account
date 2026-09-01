using Dapper;

namespace YaeaY.Account.Infrastructure.ReadModels;

public sealed class UserMyDataProjector(ReadModelConnectionFactory connectionFactory)
{
    public const string ProjectionName = "UserMyData.v1";

    public async Task ProjectAsync(Guid userId, Guid eventId, DateTimeOffset occurredOnUtc, CancellationToken cancellationToken)
    {
        await ProjectCoreAsync(userId, eventId, occurredOnUtc, registerCheckpoint: true, cancellationToken);
    }

    public async Task RebuildAsync(Guid userId, DateTimeOffset snapshotAtUtc, CancellationToken cancellationToken)
    {
        await ProjectCoreAsync(userId, Guid.Empty, snapshotAtUtc, registerCheckpoint: false, cancellationToken);
    }

    private async Task ProjectCoreAsync(
        Guid userId,
        Guid eventId,
        DateTimeOffset occurredOnUtc,
        bool registerCheckpoint,
        CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        if (registerCheckpoint)
        {
            const string checkpointSql = """
                INSERT INTO account_read."ProjectionCheckpoint" ("ProjectionName", "EventId", "OccurredOnUtc", "ProcessedAtUtc")
                VALUES (@ProjectionName, @EventId, @OccurredOnUtc, CURRENT_TIMESTAMP)
                ON CONFLICT ("ProjectionName", "EventId") DO NOTHING;
                """;

            var accepted = await connection.ExecuteAsync(new CommandDefinition(
                checkpointSql,
                new { ProjectionName, EventId = eventId, OccurredOnUtc = occurredOnUtc },
                transaction,
                cancellationToken: cancellationToken));

            if (accepted == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }
        }

        const string projectSql = """
            INSERT INTO account_read."UserMyData" (
                "UserId", "Email", "FullName", "BirthDate", "Gender", "Status", "CreatedAt",
                "EmailConfirmedAt", "FirstLoginAt", "LastLoginAt", "Phones", "Documents",
                "LastEventId", "LastEventOccurredOnUtc", "ProjectedAtUtc")
            SELECT
                u."Id", u."Email", u."UserName", u."BirthDate", u."Gender", u."Status", u."CreatedAt",
                u."EmailConfirmedAt", u."FirstLoginAt", u."LastLoginAt",
                COALESCE((
                    SELECT jsonb_agg(jsonb_build_object(
                        'Id', p."Id", 'CallingCode', p."CallingCode", 'Country', p."RegionCode",
                        'AreaCode', p."AreaCode", 'Number', p."PhoneNumber", 'PhoneType', p."PhoneType",
                        'IsPrimary', p."IsPrimary", 'CreatedAt', p."CreatedAt") ORDER BY p."CreatedAt")
                    FROM account_write."UserPhones" p WHERE p."UserId" = u."Id"), '[]'::jsonb),
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
                    WHERE d."UserId" = u."Id"), '[]'::jsonb),
                @EventId, @OccurredOnUtc, CURRENT_TIMESTAMP
            FROM account_write."User" u
            WHERE u."Id" = @UserId
            ON CONFLICT ("UserId") DO UPDATE SET
                "Email" = EXCLUDED."Email", "FullName" = EXCLUDED."FullName", "BirthDate" = EXCLUDED."BirthDate",
                "Gender" = EXCLUDED."Gender", "Status" = EXCLUDED."Status", "CreatedAt" = EXCLUDED."CreatedAt",
                "EmailConfirmedAt" = EXCLUDED."EmailConfirmedAt", "FirstLoginAt" = EXCLUDED."FirstLoginAt",
                "LastLoginAt" = EXCLUDED."LastLoginAt", "Phones" = EXCLUDED."Phones", "Documents" = EXCLUDED."Documents",
                "LastEventId" = EXCLUDED."LastEventId", "LastEventOccurredOnUtc" = EXCLUDED."LastEventOccurredOnUtc",
                "ProjectedAtUtc" = EXCLUDED."ProjectedAtUtc"
            WHERE account_read."UserMyData"."LastEventOccurredOnUtc" <= EXCLUDED."LastEventOccurredOnUtc";
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            projectSql,
            new { UserId = userId, EventId = eventId, OccurredOnUtc = occurredOnUtc },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }
}
