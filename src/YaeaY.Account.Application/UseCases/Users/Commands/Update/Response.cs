namespace YaeaY.Account.Application.UseCases.Users.Commands.Update;

public sealed record Response(
    Guid Id,
    IReadOnlyCollection<string> UpdatedFields,
    IReadOnlyCollection<CpfDocumentResponse> AddedCpfDocuments,
    string Message);

public sealed record CpfDocumentResponse(
    Guid UserDocumentId,
    Guid UserDocumentCpfId,
    string Number,
    string IssuerCountry,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<DocumentImageResponse> Images);

public sealed record DocumentImageResponse(
    Guid Id,
    short Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash,
    DateTimeOffset CreatedAt);
