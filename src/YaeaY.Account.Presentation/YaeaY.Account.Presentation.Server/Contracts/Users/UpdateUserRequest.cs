using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Presentation.Server.Contracts.Users;

public sealed record UpdateUserRequest(
    string? FullName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyCollection<CpfDocumentRequest>? CpfDocumentsToAdd);

public sealed record CpfDocumentRequest(
    string Number,
    IReadOnlyCollection<DocumentImageRequest>? Images);

public sealed record DocumentImageRequest(
    short Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash);
