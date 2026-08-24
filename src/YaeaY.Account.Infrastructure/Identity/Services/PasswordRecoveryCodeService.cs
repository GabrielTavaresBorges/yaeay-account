using System.Security.Cryptography;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Application.Services.Security.Models;
using YaeaY.Account.Domain.ValueObjects.Securities;

namespace YaeaY.Account.Infrastructure.Identity.Services;

public sealed class PasswordRecoveryCodeService : IPasswordRecoveryCodeService
{
    private const int Iterations = 100_000;

    public GeneratedPasswordRecoveryCode Generate()
    {
        var rawCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = RandomNumberGenerator.GetBytes(16);
        var derived = Rfc2898DeriveBytes.Pbkdf2(rawCode, salt, Iterations, HashAlgorithmName.SHA256, 32);
        var hash = $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(derived)}";
        var hashResult = PasswordRecoveryCodeHash.Create(hash);
        if (hashResult.IsFailure)
            throw new InvalidOperationException(hashResult.Error.Code);

        return new GeneratedPasswordRecoveryCode(rawCode, hashResult.Value);
    }

    public bool Matches(string rawCode, PasswordRecoveryCodeHash expectedHash)
    {
        ArgumentNullException.ThrowIfNull(expectedHash);
        if (string.IsNullOrWhiteSpace(rawCode) || rawCode.Length != 6 || rawCode.Any(character => !char.IsAsciiDigit(character)))
            return false;

        var segments = expectedHash.Value.Split('.', StringSplitOptions.None);
        if (segments.Length != 4 || segments[0] != "v1" || !int.TryParse(segments[1], out var iterations) || iterations != Iterations)
            return false;

        try
        {
            var salt = Convert.FromBase64String(segments[2]);
            var expected = Convert.FromBase64String(segments[3]);
            var candidate = Rfc2898DeriveBytes.Pbkdf2(rawCode, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(candidate, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
