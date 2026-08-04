using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordHash;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordHash
{
    private readonly string _passwordHash = string.Empty;

    public string Password => _passwordHash;

    private PasswordHash(string password)
    {
        _passwordHash = password;
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
            return Result<string>.Failure(PasswordHashErrors.Required);

        passwordHash = passwordHash.Trim();

        const int MaxLength = 1024;
        if (passwordHash.Length > MaxLength)
            return Result<string>.Failure(PasswordHashErrors.TooLong(passwordHash.Length, MaxLength));

        return Result<string>.Success(passwordHash);
    }
}
