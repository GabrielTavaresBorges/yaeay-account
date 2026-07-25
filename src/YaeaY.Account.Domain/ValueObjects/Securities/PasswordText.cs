using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;
using YaeaY.Account.Domain.Abstraction.Result;

namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed record PasswordText
{
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
        {
            return Result<string>.Failure(new Error(
               Code: "PASSWORD_NULL_EMPTY_WHITE_SPACE",
               Message: "Password cannot be null, empty or white space.",
               Category: ErrorCategory.Validation,
               Rule: ErrorRule.Required));
        }

        password = password.Trim();

        if (password.Length < 8)
        {
            return Result<string>.Failure(new Error(
             Code: "PASSWORD_TOO_SHORT",
             Message: "Password must be at least 8 chars.",
             Category: ErrorCategory.Validation,
             Rule: ErrorRule.MinimumLength));
        }

        if (!UppercaseRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Code: "PASSWORD_MISSING_UPPERCASE",
                Message: "Password must contain at least one uppercase letter.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.InvalidFormat));
        }

        if (!LowercaseRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Code: "PASSWORD_MISSING_LOWERCASE",
                Message: "Password must contain at least one lowercase letter.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.InvalidFormat));
        }

        if (!DigitRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Code: "PASSWORD_MISSING_DIGIT",
                Message: "Password must contain at least one number.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.InvalidFormat));
        }

        if (!SpecialRegex.IsMatch(password))
        {
            return Result<string>.Failure(new Error(
                Code: "PASSWORD_MISSING_SPECIAL",
                Message: "Password must contain at least one special character.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.InvalidFormat));
        }

        const int MaxLength = 256;
        if (password.Length > MaxLength)
        {
            return Result<string>.Failure(new Error(
                Code: "PASSWORD_TOO_LONG",
                Message: $"Password is too long. Maximum allowed length is {MaxLength} characters.",
                Category: ErrorCategory.Validation,
                Rule: ErrorRule.MaximumLength));
        }

        return Result<string>.Success(password);
    }
}
