namespace YaeaY.Account.Domain.ValueObjects.Securities;

public sealed class GeneratedEmailConfirmationToken
{
    private readonly string _rawToken;
    private readonly TokenHash _tokenHash;

    public GeneratedEmailConfirmationToken(
        string rawToken,
        TokenHash tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        ArgumentNullException.ThrowIfNull(tokenHash);

        _rawToken = rawToken;
        _tokenHash = tokenHash;
    }

    public string RevealRawToken() => _rawToken;

    public TokenHash GetTokenHash() => _tokenHash;

    public override string ToString() => nameof(GeneratedEmailConfirmationToken);
}
