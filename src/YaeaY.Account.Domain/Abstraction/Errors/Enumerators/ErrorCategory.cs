namespace YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

public enum ErrorCategory
{
    None,
    Validation,
    BusinessRule,
    Conflict,
    NotFound,
    Unexpected
}