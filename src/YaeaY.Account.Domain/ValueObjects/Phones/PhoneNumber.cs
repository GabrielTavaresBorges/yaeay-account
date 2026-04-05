using YaeaY.Account.Domain.Abstraction.Records;
using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Domain.ValueObjects.Phones;

public sealed record PhoneNumber
{
    private readonly CountryCallingCode _ddi;
    private readonly PhoneType _phoneType;
    private readonly string _number = string.Empty;
    private readonly string _e164 = string.Empty;

    public CountryCallingCode Ddi => _ddi;
    public PhoneType PhoneType => _phoneType;
    public string Number => _number;
    public string E164 => _e164;

    private PhoneNumber(CountryCallingCode ddi, PhoneType phoneType, string number)
    {
        _ddi = ddi;
        _phoneType = phoneType;
        _number = number;
        _e164 = $"+{(int)ddi}{number}";
    }

    public static Result<PhoneNumber> Create(CountryCallingCode ddi, PhoneType type, string inputNumber)
    {
        var validatedPhoneNumber = ValidatePhoneNumber(ddi, type, inputNumber);

        if (validatedPhoneNumber.IsFailure)
            return Result<PhoneNumber>.Failure(validatedPhoneNumber.Error);

        var phoneNumber = new PhoneNumber(ddi, type, validatedPhoneNumber.Value);

        return Result<PhoneNumber>.Success(phoneNumber);
    }

    private static Result<string> ValidatePhoneNumber(CountryCallingCode ddi, PhoneType type, string inputNumber)
    {
        if (type == PhoneType.Unknown)
        {
            return Result<string>.Failure(new Error(
                Identifier: "PHONE_TYPE_UNKNOWN",
                Message: "Phone type cannot be unknown."));
        }

        if (string.IsNullOrWhiteSpace(inputNumber))
        {
            return Result<string>.Failure(new Error(
                Identifier: "PHONE_NULL_EMPTY_WHITE_SPACE",
                Message: "Phone number cannot be null, empty or white space."));
        }

        // Normalização simples: manter apenas dígitos do número local (sem +, sem DDI)
        var digits = new string(inputNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return Result<string>.Failure(new Error(
                Identifier: "PHONE_INVALID",
                Message: "Phone number must contain digits."));
        }

        // Validação mínima por país (bem simples; depois você troca por IPhoneRules)
        var validation = ValidateByCountry(ddi, type, digits);
        if (validation.IsFailure)
        {
            return Result<string>.Failure(validation.Error);
        }        

        return Result<string>.Success(digits);
    }

    private static Result<bool> ValidateByCountry(CountryCallingCode ddi, PhoneType type, string digits)
    {
        // regras iniciais (exemplos simples)
        if (ddi == CountryCallingCode.Brazil)
        {
            // BR: DDD(2) + número(8/9). Mobile costuma ter 11, fixo 10.
            if (type == PhoneType.Mobile && digits.Length != 11)
                return Result<bool>.Failure(new Error(
                    Identifier: "BR_MOBILE_LEN",
                    Message: "Brazil mobile must have 11 digits (DDD + 9-digit)."));

            if (type == PhoneType.Landline && digits.Length != 10)
                return Result<bool>.Failure(new Error(
                    Identifier: "BR_LANDLINE_LEN",
                    Message: "Brazil landline must have 10 digits (DDD + 8-digit)."));

            return Result<bool>.Success(true);
        }

        if (ddi == CountryCallingCode.UnitedStates)
        {
            // EUA: 10 dígitos (regra inicial)
            if (digits.Length != 10)
                return Result<bool>.Failure(new Error(
                    Identifier: "US_PHONE_LEN",
                    Message: "US phone must have 10 digits (initial rule)."));

            return Result<bool>.Success(true);
        }

        // fallback genérico: E.164 total é 15, mas aqui é national; vamos ser conservadores
        if (digits.Length < 6 || digits.Length > 15)
            return Result<bool>.Failure(new Error(
                Identifier: "PHONE_LEN",
                Message: "Phone number must have 6..15 digits (initial rule)."));

        return Result<bool>.Success(true);
    }
}

