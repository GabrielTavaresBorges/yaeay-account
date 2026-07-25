using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.PasswordText;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordText
{
    private const int MinimumLength = 8;
    private const int MaximumLength = 256;

    private readonly string _password = string.Empty;

    public string Password => _password;

    private PasswordText(string password)
    {
        _password = password;
    }

    private static readonly Regex UppercaseRegex = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex LowercaseRegex = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex SpecialRegex = new("[^A-Za-z0-9]", RegexOptions.Compiled);

    public static Result<PasswordText> Create(string password)
    {
        var validatedPasswordText = ValidatePasswordText(password);

        if (validatedPasswordText.IsFailure)
            return Result<PasswordText>.Failure(validatedPasswordText.Error);

        var passwordText = new PasswordText(validatedPasswordText.Value);

        return Result<PasswordText>.Success(passwordText);
    }

    private static Result<string> ValidatePasswordText(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<string>.Failure(PasswordTextErrors.Required);

        password = password.Trim();

        if (password.Length < MinimumLength)
            return Result<string>.Failure(
                PasswordTextErrors.TooShort(password.Length, MinimumLength));

        if (!UppercaseRegex.IsMatch(password))
            return Result<string>.Failure(PasswordTextErrors.MissingUppercase);

        if (!LowercaseRegex.IsMatch(password))
            return Result<string>.Failure(PasswordTextErrors.MissingLowercase);

        if (!DigitRegex.IsMatch(password))
            return Result<string>.Failure(PasswordTextErrors.MissingDigit);

        if (!SpecialRegex.IsMatch(password))
            return Result<string>.Failure(PasswordTextErrors.MissingSpecialCharacter);

        if (password.Length > MaximumLength)
            return Result<string>.Failure(
                PasswordTextErrors.TooLong(password.Length, MaximumLength));

        return Result<string>.Success(password);
    }
}
