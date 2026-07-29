using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.FullName;

namespace YaeaY.Account.Domain.ValueObjects.Names;

public sealed record FullName
{
    private const int MinimumLength = 2;
    private const int MaximumLength = 100;

    private static readonly Regex WhiteSpaceSequenceRegex = new(@"\s+", RegexOptions.Compiled);

    private readonly string _name = string.Empty;

    public string Name => _name;

    private FullName(string name)
    {
        _name = name;
    }

    public static Result<FullName> Create(string name)
    {
        var validatedName = ValidateName(name);

        if (validatedName.IsFailure)
            return Result<FullName>.Failure(validatedName.Error);

        var fullName = new FullName(validatedName.Value);

        return Result<FullName>.Success(fullName);
    }

    private static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<string>.Failure(FullNameErrors.Required);

        name = WhiteSpaceSequenceRegex.Replace(name.Trim(), " ");

        if (name.Length < MinimumLength)
            return Result<string>.Failure(FullNameErrors.TooShort(name.Length, MinimumLength));

        if (name.Length > MaximumLength)
            return Result<string>.Failure(FullNameErrors.TooLong(name.Length, MaximumLength));

        return Result<string>.Success(name);
    }
}
