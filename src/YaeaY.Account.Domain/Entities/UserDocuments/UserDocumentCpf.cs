using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Errors.UserDocuments;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.Entities.UserDocuments;

public sealed class UserDocumentCpf : Entity
{
    private Cpf _cpf = null!;
    public Cpf Cpf => _cpf;

    private UserDocumentCpf() { }
    private UserDocumentCpf(Cpf cpf) => _cpf = cpf;

    internal static UserDocumentCpf Create(Cpf cpf)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        return new UserDocumentCpf(cpf);
    }

    internal bool Update(Cpf cpf)
    {
        if (cpf is null)
            throw new DomainException(UserDocumentErrors.CpfRequired);

        if (_cpf.Number == cpf.Number)
            return false;

        _cpf = cpf;
        return true;
    }
}
