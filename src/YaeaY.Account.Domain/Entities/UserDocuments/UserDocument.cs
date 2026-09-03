using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.Entities.UserDocuments;

public sealed class UserDocument : Entity
{
    private const int MaximumImages = 5;

    private DocumentType _documentType;
    private string _issuerCountry = string.Empty;
    private bool _isVerified;
    private DateTimeOffset? _verifiedAt;
    private DateTimeOffset _createdAt;
    private UserDocumentCpf? _cpf;
    private UserDocumentRg? _rg;
    private readonly List<UserDocumentImage> _images = new();

    public DocumentType DocumentType => _documentType;
    public string IssuerCountry => _issuerCountry;
    public bool IsVerified => _isVerified;
    public DateTimeOffset? VerifiedAt => _verifiedAt;
    public DateTimeOffset CreatedAt => _createdAt;
    public UserDocumentCpf? Cpf => _cpf;
    public UserDocumentRg? Rg => _rg;
    public IReadOnlyCollection<UserDocumentImage> Images => _images.AsReadOnly();

    private UserDocument() { }

    private UserDocument(Cpf cpf)
    {
        _documentType = DocumentType.Cpf;
        _issuerCountry = "BR";
        _isVerified = false;
        _verifiedAt = null;
        _createdAt = DateTimeOffset.UtcNow;
        _cpf = UserDocumentCpf.Create(cpf);
    }

    private UserDocument(Rg rg)
    {
        _documentType = DocumentType.Rg;
        _issuerCountry = "BR";
        _isVerified = false;
        _verifiedAt = null;
        _createdAt = DateTimeOffset.UtcNow;
        _rg = UserDocumentRg.Create(rg);
    }

    #region CPF
    internal static UserDocument CreateFromCpf(Cpf cpf, IEnumerable<UserDocumentImage>? images = null)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        var document = new UserDocument(cpf);

        if (images is not null)
        {
            foreach (var image in images)
                document.AddImage(image);
        }

        return document;
    }

    internal bool UpdateCpf(Cpf cpf, IEnumerable<UserDocumentImage>? images)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        var updatedImages = images?.ToArray() ?? [];
        var sameImages = _images.Count == updatedImages.Length
            && _images.OrderBy(image => image.Position).Select(image => image.StorageObjectKey)
                .SequenceEqual(updatedImages.OrderBy(image => image.Position).Select(image => image.StorageObjectKey), StringComparer.Ordinal);

        var cpfChanged = _cpf is null || _cpf.Update(cpf);
        if (!cpfChanged && sameImages)
            return false;

        var imagesByPosition = updatedImages.ToDictionary(image => image.Position);
        foreach (var existingImage in _images.ToArray())
        {
            if (imagesByPosition.Remove(existingImage.Position, out var replacement))
                existingImage.Update(replacement);
            else
                _images.Remove(existingImage);
        }

        foreach (var image in imagesByPosition.Values)
            AddImage(image);

        return true;
    }
    #endregion

    #region RG
    internal static UserDocument CreateFromRg(Rg rg, IEnumerable<UserDocumentImage>? images = null)
    {
        if (rg is null)
            throw new DomainException(UserDocumentErrors.RgRequired);

        var document = new UserDocument(rg);
        if (images is not null)
        {
            foreach (var image in images)
                document.AddImage(image);
        }

        return document;
    }

    internal bool UpdateRg(Rg rg, IEnumerable<UserDocumentImage>? images)
    {
        if (rg is null)
            throw new DomainException(UserDocumentErrors.RgRequired);

        var updatedImages = images?.ToArray() ?? [];
        var sameImages = _images.Count == updatedImages.Length
            && _images.OrderBy(image => image.Position).Select(image => image.StorageObjectKey)
                .SequenceEqual(updatedImages.OrderBy(image => image.Position).Select(image => image.StorageObjectKey), StringComparer.Ordinal);

        var rgChanged = _rg is null || _rg.Update(rg);
        if (!rgChanged && sameImages)
            return false;

        _images.Clear();
        foreach (var image in updatedImages)
            AddImage(image);

        return true;
    }
    #endregion

    internal void AddImage(UserDocumentImage image)
    {
        if (image is null)
            throw new DomainException(UserDocumentErrors.ImageRequired);

        if (_images.Count >= MaximumImages)
            throw new DomainException(UserDocumentErrors.ImageLimitExceeded);

        if (_images.Any(existing => existing.Position == image.Position))
            throw new DomainException(UserDocumentErrors.ImagePositionAlreadyExists);

        if (_images.Any(existing =>
                string.Equals(existing.StorageObjectKey, image.StorageObjectKey, StringComparison.Ordinal)))
        {
            throw new DomainException(UserDocumentErrors.ImageStorageObjectKeyAlreadyExists);
        }

        _images.Add(image);
    }
}
