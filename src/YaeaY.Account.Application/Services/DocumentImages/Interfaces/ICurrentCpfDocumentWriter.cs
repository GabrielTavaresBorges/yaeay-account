namespace YaeaY.Account.Application.Services.DocumentImages.Interfaces;

public interface ICurrentCpfDocumentWriter
{
    Task<CpfDocumentWriteResult> ReplaceAsync(
        Guid userId,
        string cpfNumber,
        IReadOnlyCollection<CpfDocumentImageWriteModel> images,
        CancellationToken cancellationToken);
}

public sealed record CpfDocumentImageWriteModel(
    short Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash);

public sealed record CpfDocumentWriteResult(
    Guid DocumentId,
    Guid CpfId,
    string Number,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<CpfDocumentImageWriteResult> Images);

public sealed record CpfDocumentImageWriteResult(
    Guid Id,
    short Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash,
    DateTimeOffset CreatedAt);
