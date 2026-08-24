namespace YaeaY.Account.Domain.Policies.PasswordRecoveries;

public interface IPasswordRecoveryPolicy
{
    TimeSpan CodeLifetime { get; }
    TimeSpan ResetAuthorizationLifetime { get; }
    TimeSpan ResendInterval { get; }
    TimeSpan RequestWindow { get; }
    int MaximumFailedAttempts { get; }
    int MaximumRequestsPerWindow { get; }
}
