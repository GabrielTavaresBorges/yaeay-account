using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Application.Services.Security.Models;

public sealed class GeneratedPasswordRecoveryCode(string rawCode, PasswordRecoveryCodeHash codeHash)
{
    private readonly string _rawCode = rawCode;
    public PasswordRecoveryCodeHash CodeHash { get; } = codeHash;
    public string RevealRawCode() => _rawCode;
    public override string ToString() => nameof(GeneratedPasswordRecoveryCode);
}
