using System.Text.RegularExpressions;
using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Emails;

namespace YaeaY.Account.Domain.ValueObjects.Emails;

public sealed partial record Email
{
    private const int MaximumLength = 254;

    private readonly string _emailAddress = string.Empty;

    public string EmailAddress => _emailAddress;

    private Email(string emailAddress)
    {
        _emailAddress = emailAddress;
    }

    public static Result<Email> Create(string emailAddress)
    {
        var validatedEmail = ValidateEmail(emailAddress);

        if (validatedEmail.IsFailure)
            return Result<Email>.Failure(validatedEmail.Error);

        var email = new Email(validatedEmail.Value);

        return Result<Email>.Success(email);
    }

    private static Result<string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<string>.Failure(EmailErrors.Required);

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaximumLength)
            return Result<string>.Failure(EmailErrors.TooLong(email.Length, MaximumLength));

        if (!EmailRegex().IsMatch(email))
            return Result<string>.Failure(EmailErrors.InvalidFormat);

        return Result<string>.Success(email);
    }

    /// <summary>
    /// Valida a estrutura sintática do endereço de email normalizado.
    /// Exige conteúdo antes e depois de '@', não permite espaços
    /// e exige pelo menos um ponto na parte do domínio.
    /// Não representa uma implementação completa das RFCs de email.
    /// </summary>
    [GeneratedRegex(@"^(?!.*\.\.)[a-z0-9](?:[a-z0-9._%+-]*[a-z0-9])?@[a-z0-9](?:[a-z0-9-]*[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
