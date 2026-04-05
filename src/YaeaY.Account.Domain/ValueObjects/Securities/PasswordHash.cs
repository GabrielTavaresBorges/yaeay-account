using YaeaY.Account.Domain.Abstraction.Records;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordHash
{
    private readonly string _passwordHash = string.Empty;

    public string Password => _passwordHash;

    private PasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
    }

    public static Result<PasswordHash> Create(string hashed)
    {
        var validatedPasswordHash = ValidatePasswordHash(hashed);

        if (validatedPasswordHash.IsFailure)
            return Result<PasswordHash>.Failure(validatedPasswordHash.Error);

        var passwordHash = new PasswordHash(validatedPasswordHash.Value);

        return Result<PasswordHash>.Success(passwordHash);
    }

    private static Result<string> ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result<string>.Failure(new Error(
                 Identifier: "PASSWORD_HASH_NULL_EMPTY_WHITE_SPACE",
                 Message: "Password hash cannot be null, empty or white space."));
        }

        passwordHash = passwordHash.Trim();

        const int MaxLength = 1024;
        if (passwordHash.Length > MaxLength)
        {
            return Result<string>.Failure(new Error(
               Identifier: "PASSWORD_HASH_TOO_LONG",
               Message: $"Password hash is too long. Current length: {passwordHash.Length}. Max: {MaxLength}."));
        }
            
        return Result<string>.Success(passwordHash);
    }
}
