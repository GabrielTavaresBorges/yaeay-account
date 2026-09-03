using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Application.Services.DocumentImages.Interfaces;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Persistence.DocumentImages;

public sealed class CurrentCpfDocumentWriter(AppDbContext context) : ICurrentCpfDocumentWriter
{
    public async Task<CpfDocumentWriteResult> ReplaceAsync(
        Guid userId,
        string cpfNumber,
        IReadOnlyCollection<CpfDocumentImageWriteModel> images,
        CancellationToken cancellationToken)
    {
        const string cpfType = "Cpf";
        var documentId = Guid.NewGuid();
        var cpfId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM account_write."UserDocumentImages" AS image
            USING account_write."UserDocuments" AS document
            WHERE image."UserDocumentId" = document."Id"
              AND document."UserId" = {userId}
              AND document."DocumentType" = {cpfType};
            """, cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM account_write."UserDocumentCpf" AS cpf
            USING account_write."UserDocuments" AS document
            WHERE cpf."UserDocumentId" = document."Id"
              AND document."UserId" = {userId}
              AND document."DocumentType" = {cpfType};
            """, cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DELETE FROM account_write."UserDocuments"
            WHERE "UserId" = {userId} AND "DocumentType" = {cpfType};
            """, cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO account_write."UserDocuments"
                ("Id", "UserId", "DocumentType", "IssuerCountry", "IsVerified", "VerifiedAt", "CreatedAt")
            VALUES ({documentId}, {userId}, {cpfType}, {"BR"}, FALSE, NULL, {createdAt});
            """, cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO account_write."UserDocumentCpf" ("Id", "UserDocumentId", "Number")
            VALUES ({cpfId}, {documentId}, {cpfNumber});
            """, cancellationToken);

        var storedImages = new List<CpfDocumentImageWriteResult>(images.Count);
        foreach (var image in images.OrderBy(item => item.Position))
        {
            var imageId = Guid.NewGuid();
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO account_write."UserDocumentImages"
                    ("Id", "UserDocumentId", "Position", "StorageObjectKey", "OriginalFileName", "ContentType", "FileSizeBytes", "Sha256Hash", "CreatedAt")
                VALUES ({imageId}, {documentId}, {image.Position}, {image.StorageObjectKey}, {image.OriginalFileName}, {image.ContentType}, {image.FileSizeBytes}, {image.Sha256Hash}, {createdAt});
                """, cancellationToken);
            storedImages.Add(new CpfDocumentImageWriteResult(
                imageId, image.Position, image.StorageObjectKey, image.OriginalFileName,
                image.ContentType, image.FileSizeBytes, image.Sha256Hash, createdAt));
        }

        return new CpfDocumentWriteResult(documentId, cpfId, cpfNumber, createdAt, storedImages);
    }
}
