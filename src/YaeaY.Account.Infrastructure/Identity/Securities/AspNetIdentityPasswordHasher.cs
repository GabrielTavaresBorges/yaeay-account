using Microsoft.AspNetCore.Identity;
using YaeaY.Account.Application.Services.Security.Interfaces;

using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Infrastructure.Identity.Securities;

public sealed class AspNetIdentityPasswordHasher : IPasswordHasher
{
    // O PasswordHasher do ASP.NET Identity já implementa:
    // - salt por senha
    // - formato versionado
    // - comparação segura
    private readonly PasswordHasher<object> _hasher = new();

    public Result<PasswordHash> Hash(PasswordText password)
    {
        ArgumentNullException.ThrowIfNull(password);

        // O parâmetro "user" aqui não é necessário no nosso caso
        var hashedPassword = _hasher.HashPassword(null!, password.Password);

        return PasswordHash.Create(hashedPassword);
    }

    public bool Verify(PasswordHash passwordHash, string providedPassword)
    {
        if (passwordHash is null)
            return false;

        if (string.IsNullOrWhiteSpace(providedPassword))
            return false;

        var result = _hasher.VerifyHashedPassword(
            null!,
            passwordHash.Password,
            providedPassword);

        // SuccessRehashNeeded = senha correta, mas o hash deveria ser atualizado
        // (por exemplo, após upgrade de parâmetros/versão). Você pode tratar isso
        // futuramente no login para re-hash automático.
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
