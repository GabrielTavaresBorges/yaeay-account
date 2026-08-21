using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordRecoveryChallenges;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordRecoveryCodeHash
{
    private readonly string _value = string.Empty;
    public string Value => _value;

    private PasswordRecoveryCodeHash(string value) => _value = value;

    public static Result<PasswordRecoveryCodeHash> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PasswordRecoveryCodeHash>.Failure(PasswordRecoveryChallengeErrors.CodeHashRequired);

        value = value.Trim();
        if (value.Length > 128)
            return Result<PasswordRecoveryCodeHash>.Failure(PasswordRecoveryChallengeErrors.CodeHashRequired);

        return Result<PasswordRecoveryCodeHash>.Success(new PasswordRecoveryCodeHash(value));
    }
}
