namespace YaeaY.Account.Application.UseCases.EmailConfirmations.Commands.IssueInitialToken;

public sealed class Response
{
    private readonly string _rawToken;

    public Guid TokenId { get; }
    public string ToEmail { get; }
    public string FullName { get; }
    public DateTimeOffset ExpiresAt { get; }

    public Response(
        Guid tokenId,
        string toEmail,
        string fullName,
        string rawToken,
        DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        TokenId = tokenId;
        ToEmail = toEmail;
        FullName = fullName;
        ExpiresAt = expiresAt;
        _rawToken = rawToken;
    }

    public string RevealRawToken() => _rawToken;

    public override string ToString() => nameof(Response);
}
