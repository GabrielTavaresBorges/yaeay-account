using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Abstraction.Errors;

public sealed record Error(
    string Code,
    string Message,
    ErrorCategory Category,
    ErrorRule Rule)
{
    public static readonly Error None = new(
        Code: string.Empty,
        Message: string.Empty,
        Category: default,
        Rule: default);

    public bool IsNone =>
        string.IsNullOrWhiteSpace(Code) &&
        string.IsNullOrWhiteSpace(Message);
}
