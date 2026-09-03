namespace YaeaY.Account.Application.UseCases.Users.Commands.UploadCpfDocumentImage;

public sealed record Response(
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash);
