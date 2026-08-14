namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

public sealed class Response
{
    private readonly string _rawToken;

    public Guid TokenId { get; }
    public DateTimeOffset ExpiresAt { get; }

    public Response(
        Guid tokenId,
        string rawToken,
        DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        TokenId = tokenId;
        ExpiresAt = expiresAt;
        _rawToken = rawToken;
    }

    public string RevealRawToken() => _rawToken;

    public override string ToString() => nameof(Response);
}
