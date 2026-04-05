using YaeaY.Account.Domain.Abstraction.Records;

namespace YaeaY.Account.Domain.ValueObjects.Names;

public sealed record UserName
{
    private readonly string _name = string.Empty;

    public string Name => _name;

    private UserName(string name)
    {
        _name = name;
    }

    public static Result<UserName> Create(string name)
    {
        var validatedName = ValidateName(name);

        if (validatedName.IsFailure)
            return Result<UserName>.Failure(validatedName.Error);

        var userName = new UserName(validatedName.Value);

        return Result<UserName>.Success(userName);
    }

    private static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<string>.Failure(new Error(
             Identifier: "USER_NAME_NULL_EMPTY_WHITE_SPACE",
             Message: "Name cannot be null, empty or white space."));
        }

        name = name.Trim();

        if (name.Length < 2 || name.Length > 100)
        {
            return Result<string>.Failure(new Error(
               Identifier: "USER_NAME_INVALID_LENGTH",
               Message: "Name must be between 2 and 100 characters."));
        }

        return Result<string>.Success(name);
    }
}
