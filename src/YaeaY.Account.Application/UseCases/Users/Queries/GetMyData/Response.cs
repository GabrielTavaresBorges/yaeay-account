namespace YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

public sealed record Response(
    Guid UserId,
    string Email,
    string FullName,
    DateOnly BirthDate,
    string Gender,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EmailConfirmedAt,
    DateTimeOffset? FirstLoginAt,
    DateTimeOffset? LastLoginAt,
    IReadOnlyCollection<PhoneResponse> Phones,
    IReadOnlyCollection<DocumentResponse> Documents,
    DateTimeOffset ProjectedAtUtc);

public sealed record PhoneResponse(
    Guid Id,
    string CallingCode,
    string Country,
    string AreaCode,
    string Number,
    string PhoneType,
    bool IsPrimary,
    DateTimeOffset CreatedAt);

public sealed record DocumentResponse(
    Guid Id,
    string Type,
    string? Number,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<DocumentImageResponse> Images);

public sealed record DocumentImageResponse(
    Guid Id,
    int Position,
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Hash,
    DateTimeOffset CreatedAt);
