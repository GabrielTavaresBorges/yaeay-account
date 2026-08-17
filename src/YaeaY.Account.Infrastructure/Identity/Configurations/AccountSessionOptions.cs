namespace YaeaY.Account.Infrastructure.Identity.Configurations;

public sealed class AccountSessionOptions
{
    public const string SectionName = "Authentication:Session";

    public int IdleTimeoutInMinutes { get; init; } = 30;
    public int RememberMeDurationInDays { get; init; } = 14;
    public int SecurityStampValidationIntervalInMinutes { get; init; } = 5;
    public int MaxFailedAccessAttempts { get; init; } = 5;
    public int LockoutDurationInMinutes { get; init; } = 15;
}
