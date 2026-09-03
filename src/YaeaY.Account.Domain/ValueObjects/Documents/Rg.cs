using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.Rg;

namespace YaeaY.Account.Domain.ValueObjects.Documents;

public sealed partial record Rg
{
    private static readonly HashSet<string> BrazilianStates = [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"];

    private readonly string _number = string.Empty;
    private readonly DateOnly _issuedAt;
    private readonly string _issuingAuthority = string.Empty;
    private readonly string _issuingState = string.Empty;

    public string Number => _number;
    public DateOnly IssuedAt => _issuedAt;
    public string IssuingAuthority => _issuingAuthority;
    public string IssuingState => _issuingState;

    private Rg() { }
    private Rg(
        string number,
        DateOnly issuedAt,
        string issuingAuthority,
        string issuingState)
    {
        _number = number;
        _issuedAt= issuedAt;
        _issuingAuthority = issuingAuthority;
        _issuingState = issuingState;
    }

    public static Result<Rg> Create(string number, DateOnly issuedAt, string issuingAuthority, string issuingState)
    {
        var validated = Validate(number, issuedAt, issuingAuthority, issuingState);

        if (validated.IsFailure)
            return Result<Rg>.Failure(validated.Error);       

        var rg = new Rg(
            validated.Value.Number,
            validated.Value.IssuedAt,
            validated.Value.IssuingAuthority,
            validated.Value.IssuingState);

        return Result<Rg>.Success(rg);
    }

    private static Result<ValidatedRg> Validate(string number, DateOnly issuedAt, string issuingAuthority, string issuingState)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result<ValidatedRg>.Failure(RgErrors.NumberRequired);

        if (issuedAt == default)
            return Result<ValidatedRg>.Failure(RgErrors.IssueDateRequired);

        if (string.IsNullOrWhiteSpace(issuingAuthority))
            return Result<ValidatedRg>.Failure(RgErrors.IssuingAuthorityRequired);

        var normalizedState = issuingState?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!BrazilianStates.Contains(normalizedState))
            return Result<ValidatedRg>.Failure(RgErrors.IssuingStateInvalid);

        return Result<ValidatedRg>.Success(new ValidatedRg(
            number.Trim().ToUpperInvariant(),
            issuedAt,
            issuingAuthority.Trim(),
            normalizedState));
    }

    private sealed record ValidatedRg(string Number, DateOnly IssuedAt, string IssuingAuthority, string IssuingState);
}
