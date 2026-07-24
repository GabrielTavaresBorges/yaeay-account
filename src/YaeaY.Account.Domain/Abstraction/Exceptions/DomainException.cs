using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Abstraction.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public ErrorCategory Category { get; }
    public ErrorRule Rule { get; }

    public DomainException(
        string code,
        string message,
        ErrorCategory category = ErrorCategory.BusinessRule,
        ErrorRule rule = ErrorRule.InvariantViolation)
        : base(message)
    {
        Code = code;
        Category = category;
        Rule = rule;
    }

    public DomainException(Error error) : base(error.Message)
    {
        Code = error.Code;
        Category = error.Category;
        Rule = error.Rule;
    }
}
