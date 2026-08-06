using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Abstraction.Exceptions;
public class DomainException : Exception
{
    public Error Error { get; }

    public string Code => Error.Code;
    public ErrorCategory Category => Error.Category;
    public ErrorRule Rule => Error.Rule;

    /// <summary>
    /// Cria uma exceção de domínio a partir das informações individuais
    /// necessárias para construir um <see cref="Error"/>.
    ///
    /// Este construtor é útil quando o chamador ainda não possui uma
    /// instância de <see cref="Error"/>.
    /// </summary>
    public DomainException(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.BusinessRule,
        ErrorRule rule = ErrorRule.InvariantViolation)
        : this(
            new Error(
                Code: code,
                Message: message,
                Category: category,
                Rule: rule),
            innerException: null)
    {
    }

    /// <summary>
    /// Cria uma exceção de domínio a partir de um erro conhecido.
    ///
    /// Deve ser utilizado quando não existe uma exceção técnica anterior
    /// que precise ser preservada.
    /// </summary>
    public DomainException(Error error) : this(error, innerException: null) {}

    /// <summary>
    /// Cria uma exceção de domínio a partir de um erro conhecido,
    /// preservando a exceção original que causou o problema.
    ///
    /// Este construtor pode ser utilizado pela Infrastructure para traduzir
    /// uma exceção técnica, como DbUpdateException ou PostgresException,
    /// para um erro conhecido pelo domínio.
    /// </summary>
    public DomainException(Error error, Exception? innerException)
        : base(
            (error ?? throw new ArgumentNullException(nameof(error))).Message, innerException)
    {
        Error = error;
    }
}