using YaeaY.Account.Domain.Abstraction.Errors;
using YaeaY.Account.Domain.Abstraction.Errors.Enumerators;

namespace YaeaY.Account.Domain.Errors.Cpf;

public static class CpfErrors
{
    public static readonly Error NumberRequired = new(
        Code: "CPF_NUMBER_NULL_EMPTY_WHITE_SPACE",
        Message: "CPF number cannot be null, empty or white space.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.Required);

    public static readonly Error NumberInvalidLength = new(
        Code: "CPF_NUMBER_INVALID_LENGTH",
        Message: "CPF number must be 11 digits long and contain only numbers.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidFormat);

    public static readonly Error NumberChecksumInvalid = new(
        Code: "CPF_NUMBER_CHECKSUM_INVALID",
        Message: "CPF failed validation.",
        Category: ErrorCategory.Validation,
        Rule: ErrorRule.InvalidValue);
}
