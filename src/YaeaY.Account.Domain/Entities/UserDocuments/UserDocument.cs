using YaeaY.Account.Domain.Abstraction.Entities;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Documents;

namespace YaeaY.Account.Domain.Entities.UserDocuments;

public sealed class UserDocument : Entity
{
    private DocumentType _documentType;
    private string _documentNumber = string.Empty;
    private DateTimeOffset _createdAt;

    public DocumentType DocumentType => _documentType;
    public string DocumentNumber => _documentNumber;
    public DateTimeOffset CreatedAt => _createdAt;

    private UserDocument() { }

    private UserDocument(DocumentType documentType, string documentNumber)
    {
        _documentType = documentType;
        _documentNumber = documentNumber;

        _createdAt = DateTimeOffset.UtcNow;
    }

    internal static UserDocument CreateFromCpf(Cpf cpf)
    {
        if (cpf is null)
            throw new DomainException(
                message: "Cpf cannot be null.",
                code: "CPF_NULL");

        return new UserDocument(DocumentType.Cpf, cpf.Number);
    }
}
