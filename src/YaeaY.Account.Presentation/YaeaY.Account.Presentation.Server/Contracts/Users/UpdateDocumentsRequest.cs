namespace YaeaY.Account.Presentation.Server.Contracts.Users;

public sealed record UpdateDocumentsRequest(IReadOnlyCollection<CpfDocumentRequest>? CpfDocumentsToAdd, IReadOnlyCollection<RgDocumentRequest>? RgDocumentsToAdd);
public sealed record CpfDocumentRequest(string Number, IReadOnlyCollection<DocumentImageRequest>? Images);
public sealed record RgDocumentRequest(string Number, DateOnly IssuedAt, string IssuingAuthority, string IssuingState, IReadOnlyCollection<DocumentImageRequest>? Images);
public sealed record DocumentImageRequest(short Position, string StorageObjectKey, string OriginalFileName, string ContentType, long FileSizeBytes, string Sha256Hash);
