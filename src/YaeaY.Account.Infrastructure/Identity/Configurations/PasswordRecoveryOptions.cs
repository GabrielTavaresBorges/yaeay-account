namespace YaeaY.Account.Infrastructure.Identity.Configurations;

public sealed class PasswordRecoveryOptions
{
    public const string SectionName = "PasswordRecovery";

    public int CodeLifetimeInMinutes { get; init; } = 2;
    public int ResetAuthorizationLifetimeInMinutes { get; init; } = 10;
    public int ResendIntervalInSeconds { get; init; } = 120;
    public int RequestWindowInMinutes { get; init; } = 60;
    public int MaximumFailedAttempts { get; init; } = 5;
    public int MaximumRequestsPerWindow { get; init; } = 5;
}
