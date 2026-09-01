namespace YaeaY.Account.Application.Services.DocumentImages.Interfaces;

public interface IDocumentImageStorage
{
    Task<StoredDocumentImage> StoreCpfImageAsync(
        Guid userId,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string storageObjectKey, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storageObjectKey, CancellationToken cancellationToken);

    Task DeleteAsync(string storageObjectKey, CancellationToken cancellationToken);
}

public sealed record StoredDocumentImage(
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash);
