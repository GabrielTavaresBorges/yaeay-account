using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Abstraction.Exceptions;

public class DomainException : Exception
{
    public Error Error { get; }
    public string Code => Error.Code;
    public ErrorCategory Category => Error.Category;
    public ErrorRule Rule => Error.Rule;

    public DomainException(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.BusinessRule,
        ErrorRule rule = ErrorRule.InvariantViolation)
        : this(new Error(code, message, category, rule)) { }

    public DomainException(Error error) : base(error.Message)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }
}
