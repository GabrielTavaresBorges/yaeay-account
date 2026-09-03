using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.Entities.UserDocuments;

public sealed class UserDocumentRg : Entity
{
    private Rg _rg = null!;
    public Rg Rg => _rg;
    private UserDocumentRg() { }
    private UserDocumentRg(Rg rg) => _rg = rg;

    internal static UserDocumentRg Create(Rg rg)
    {
        if (rg is null)
            throw new DomainException(UserDocumentErrors.RgRequired);

        return new UserDocumentRg(rg);
    }

    internal bool Update(Rg rg)
    {
        if (rg is null)
            throw new DomainException(UserDocumentErrors.RgRequired);

        if (_rg == rg)
            return false;

        _rg = rg;

        return true;
    }
}
