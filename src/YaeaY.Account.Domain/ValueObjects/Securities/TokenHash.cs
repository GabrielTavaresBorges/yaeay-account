using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record TokenHash
{
    private readonly string _token = string.Empty;

    public string Token=> _token;

    private TokenHash(string token)
    {
        _token = token;
    }

    public static Result<TokenHash> Create(string tokenHash)
    {
        var validatedTokenHash = ValidateTokenHash(tokenHash);
        if (validatedTokenHash.IsFailure)
            return Result<TokenHash>.Failure(validatedTokenHash.Error);

        var token = new TokenHash(validatedTokenHash.Value);

        return Result<TokenHash>.Success(token);
    }

    private static Result<string> ValidateTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result<string>.Failure(new Error(
                Code: "TOKEN_HASH_NULL_EMPTY_WHITE_SPACE",
                Message: "Token hash cannot be null, empty or white space.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.Required));
        }

        tokenHash = tokenHash.Trim();

        const int MaxLength = 1024;
        if (tokenHash.Length > MaxLength)
        {
            return Result<string>.Failure(new Error(
                Code: "TOKEN_HASH_TOO_LONG",
                Message: $"Token hash is too long. Current length: {tokenHash.Length}. Max: {MaxLength}.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.MaximumLength));
        }

        return Result<string>.Success(tokenHash);
    }
}
