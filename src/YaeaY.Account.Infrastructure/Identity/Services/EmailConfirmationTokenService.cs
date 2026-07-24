using System.Security.Cryptography;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Domain.Abstraction.Exceptions;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Infrastructure.Identity.Services;

public sealed class EmailConfirmationTokenService : IEmailConfirmationTokenService
{
    public Task<GeneratedEmailConfirmationToken> GenerateTokenAsync()
    {
        const int tokenSizeInBytes = 32;

        var rawToken = GenerateSecureToken(tokenSizeInBytes);
        var tokenHash = GenerateTokenHash(rawToken);

        var tokenHashResult = TokenHash.Create(tokenHash);
        if (tokenHashResult.IsFailure)
            throw new DomainException(
                message: tokenHashResult.Error.Message,
                code: tokenHashResult.Error.Code);

        var result = new GeneratedEmailConfirmationToken(
            rawToken: rawToken,
            tokenHash: tokenHashResult.Value);

        return Task.FromResult(result);
    }

    private static string GenerateSecureToken(int sizeInBytes)
    {
        var bytes = RandomNumberGenerator.GetBytes(sizeInBytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateTokenHash(string rawToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }
}
