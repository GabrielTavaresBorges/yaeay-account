namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateDocuments;

public sealed record Response(Guid Id, IReadOnlyCollection<CpfDocumentResponse> CpfDocuments, IReadOnlyCollection<RgDocumentResponse> RgDocuments, bool HasChanges, string Message);
public sealed record CpfDocumentResponse(Guid UserDocumentId, Guid UserDocumentCpfId, string Number, string IssuerCountry, DateTimeOffset CreatedAt, IReadOnlyCollection<DocumentImageResponse> Images);
public sealed record RgDocumentResponse(Guid UserDocumentId, Guid UserDocumentRgId, string Number, DateOnly IssuedAt, string IssuingAuthority, string IssuingState, DateTimeOffset CreatedAt, IReadOnlyCollection<DocumentImageResponse> Images);
public sealed record DocumentImageResponse(Guid Id, short Position, string StorageObjectKey, string OriginalFileName, string ContentType, long FileSizeBytes, string Sha256Hash, DateTimeOffset CreatedAt);
