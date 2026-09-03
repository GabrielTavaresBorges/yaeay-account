using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateDocuments;

public sealed record Command(Guid Id, IReadOnlyCollection<CpfDocumentInput>? CpfDocumentsToAdd, IReadOnlyCollection<RgDocumentInput>? RgDocumentsToAdd) : IRequest<Result<Response>>;
public sealed record CpfDocumentInput(string Number, IReadOnlyCollection<DocumentImageInput>? Images);
public sealed record RgDocumentInput(string Number, DateOnly IssuedAt, string IssuingAuthority, string IssuingState, IReadOnlyCollection<DocumentImageInput>? Images);
public sealed record DocumentImageInput(short Position, string StorageObjectKey, string OriginalFileName, string ContentType, long FileSizeBytes, string Sha256Hash);
