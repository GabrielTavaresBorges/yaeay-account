namespace YaeaY.Account.Application.UseCases.PasswordRecoveries.Commands.IssuePasswordRecoveryCode;

public sealed class Response(Guid challengeId, string toEmail, string fullName, string? rawCode, bool shouldDeliver)
{
    private readonly string? _rawCode = rawCode;
    public Guid ChallengeId { get; } = challengeId;
    public string ToEmail { get; } = toEmail;
    public string FullName { get; } = fullName;
    public bool ShouldDeliver { get; } = shouldDeliver;
    public string RevealRawCode() => _rawCode ?? throw new InvalidOperationException("No recovery code is available.");
    public override string ToString() => nameof(Response);
}
