using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Records;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordPlainText
{
    private readonly string _password = string.Empty;

    public string Password => _password;

    private PasswordPlainText(string password)
    {
        _password = password;
    }

    private static readonly Regex UppercaseRegex = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex LowercaseRegex = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex SpecialRegex = new("[^A-Za-z0-9]", RegexOptions.Compiled);

    public static Result<PasswordPlainText> Create(string password)
    {
        var validatedPasswordPlainText = ValidatePasswordPlainText(password);

        if (validatedPasswordPlainText.IsFailure)
            return Result<PasswordPlainText>.Failure(validatedPasswordPlainText.Error);

        var passwordPlainText = new PasswordPlainText(validatedPasswordPlainText.Value);

        return Result<PasswordPlainText>.Success(passwordPlainText);
    }

    private static Result<string> ValidatePasswordPlainText(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result<string>.Failure(new Error(
               Identifier: "PASSWORD_NULL_EMPTY_WHITE_SPACE",
               Message: "Password cannot be null, empty or white space."));
        }

        password = password.Trim();

        if (password.Length < 8)
        {
            return Result<string>.Failure(new Error(
             Identifier: "PASSWORD_TOO_SHORT",
             Message: "Password must be at least 8 chars."));
        }

        if (!UppercaseRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Identifier: "PASSWORD_MISSING_UPPERCASE",
                Message: "Password must contain at least one uppercase letter."));
        }

        if (!LowercaseRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Identifier: "PASSWORD_MISSING_LOWERCASE",
                Message: "Password must contain at least one lowercase letter."));
        }

        if (!DigitRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Identifier: "PASSWORD_MISSING_DIGIT",
                Message: "Password must contain at least one number."));
        }

        if (!SpecialRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Identifier: "PASSWORD_MISSING_SPECIAL",
                Message: "Password must contain at least one special character."));
        }

        const int MaxLength = 256;
        if (password.Length > MaxLength)
        {
            return Result<string>.Failure(new Error(
                Identifier: "PASSWORD_TOO_LONG",
                Message: $"Password is too long. Maximum allowed length is {MaxLength} characters."));
        }

        return Result<string>.Success(password);
    }
}