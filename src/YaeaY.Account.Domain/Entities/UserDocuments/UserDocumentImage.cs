using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.UserDocumentImages;

namespace YaeaY.Account.Domain.Entities.UserDocuments;

public sealed partial class UserDocumentImage : Entity
{
    public const short MinimumPosition = 1;
    public const short MaximumPosition = 5;
    public const int MaximumStorageObjectKeyLength = 512;
    public const int MaximumOriginalFileNameLength = 255;
    public const int MaximumContentTypeLength = 100;

    private short _position;
    private string _storageObjectKey = string.Empty;
    private string _originalFileName = string.Empty;
    private string _contentType = string.Empty;
    private long _fileSizeBytes;
    private string _sha256Hash = string.Empty;
    private DateTimeOffset _createdAt;

    public short Position => _position;
    public string StorageObjectKey => _storageObjectKey;
    public string OriginalFileName => _originalFileName;
    public string ContentType => _contentType;
    public long FileSizeBytes => _fileSizeBytes;
    public string Sha256Hash => _sha256Hash;
    public DateTimeOffset CreatedAt => _createdAt;

    private UserDocumentImage() { }

    private UserDocumentImage(short position, string storageObjectKey, string originalFileName, string contentType, long fileSizeBytes, string sha256Hash)
    {
        _position = position;
        _storageObjectKey = storageObjectKey;
        _originalFileName = originalFileName;
        _contentType = contentType;
        _fileSizeBytes = fileSizeBytes;
        _sha256Hash = sha256Hash;
        _createdAt = DateTimeOffset.UtcNow;
    }

    public static UserDocumentImage Create(short position, string storageObjectKey, string originalFileName, string contentType, long fileSizeBytes, string sha256Hash)
    {
        if (position is < MinimumPosition or > MaximumPosition)
            throw new DomainException(UserDocumentImageErrors.PositionInvalid);

        var normalizedStorageObjectKey = storageObjectKey?.Trim() ?? string.Empty;
        if (normalizedStorageObjectKey.Length == 0)
            throw new DomainException(UserDocumentImageErrors.StorageObjectKeyRequired);
        if (normalizedStorageObjectKey.Length > MaximumStorageObjectKeyLength)
            throw new DomainException(UserDocumentImageErrors.StorageObjectKeyTooLong);

        var normalizedOriginalFileName = originalFileName?.Trim() ?? string.Empty;
        if (normalizedOriginalFileName.Length == 0)
            throw new DomainException(UserDocumentImageErrors.OriginalFileNameRequired);
        if (normalizedOriginalFileName.Length > MaximumOriginalFileNameLength)
            throw new DomainException(UserDocumentImageErrors.OriginalFileNameTooLong);

        var normalizedContentType = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalizedContentType.Length == 0)
            throw new DomainException(UserDocumentImageErrors.ContentTypeRequired);
        if (normalizedContentType.Length > MaximumContentTypeLength)
            throw new DomainException(UserDocumentImageErrors.ContentTypeTooLong);

        if (fileSizeBytes <= 0)
            throw new DomainException(UserDocumentImageErrors.FileSizeInvalid);
        var normalizedHash = sha256Hash?.Trim().ToLowerInvariant() ?? string.Empty;
        if (normalizedHash.Length == 0)
            throw new DomainException(UserDocumentImageErrors.Sha256HashRequired);
        if (!Sha256HashRegex().IsMatch(normalizedHash))
            throw new DomainException(UserDocumentImageErrors.Sha256HashInvalid);

        return new UserDocumentImage(position, normalizedStorageObjectKey, normalizedOriginalFileName, normalizedContentType, fileSizeBytes, normalizedHash);
    }

    internal bool Update(UserDocumentImage source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_position == source._position
            && _storageObjectKey == source._storageObjectKey
            && _originalFileName == source._originalFileName
            && _contentType == source._contentType
            && _fileSizeBytes == source._fileSizeBytes
            && _sha256Hash == source._sha256Hash)
        {
            return false;
        }

        _position = source._position;
        _storageObjectKey = source._storageObjectKey;
        _originalFileName = source._originalFileName;
        _contentType = source._contentType;
        _fileSizeBytes = source._fileSizeBytes;
        _sha256Hash = source._sha256Hash;
        return true;
    }

    [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256HashRegex();
}
