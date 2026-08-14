namespace YaeaY.Account.Domain.Policies.EmailConfirmations;

public interface IEmailConfirmationTokenExpirationPolicy
{
    DateTimeOffset GetInitialStageExpiration(DateTimeOffset accountCreatedAt);
}
