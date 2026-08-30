using MediatR;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.Update;

public sealed record Command(
    Guid Id,
    string? FullName,
    DateOnly? BirthDate,
    Gender? Gender,
    IReadOnlyCollection<PhoneInput>? Phones,
    IReadOnlyCollection<CpfDocumentInput>? CpfDocumentsToAdd)
    : IRequest<Result<Response>>;

public sealed record PhoneInput(
    Guid? Id,
    string CallingCode,
    string RegionCode,
    string? AreaCode,
    TelephoneType PhoneType,
    string PhoneNumber,
    bool IsPrimary);

public sealed record CpfDocumentInput(
    string Number,
    IReadOnlyCollection<DocumentImageInput>? Images);

public sealed record DocumentImageInput(
    short Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash);
