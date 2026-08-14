using Microsoft.Extensions.Configuration;
using YaeaY.Account.Domain.Policies.EmailConfirmations;

namespace YaeaY.Account.Infrastructure.Identity.Policies;

public sealed class ConfigurationEmailConfirmationTokenExpirationPolicy : IEmailConfirmationTokenExpirationPolicy
{
    private const string InitialStageMonthsKey =
        "EmailConfirmationPolicy:InitialStageEndsAfterMonths";

    private readonly int _initialStageEndsAfterMonths;

    public ConfigurationEmailConfirmationTokenExpirationPolicy(IConfiguration configuration)
    {
        var configuredValue = configuration[InitialStageMonthsKey];

        if (!int.TryParse(configuredValue, out _initialStageEndsAfterMonths) ||
            _initialStageEndsAfterMonths <= 0)
        {
            throw new InvalidOperationException(
                $"Configuration '{InitialStageMonthsKey}' must be a positive integer.");
        }
    }

    public DateTimeOffset GetInitialStageExpiration(DateTimeOffset accountCreatedAt)
    {
        if (accountCreatedAt == default)
            throw new ArgumentException("The account creation date is required.", nameof(accountCreatedAt));

        return accountCreatedAt.AddMonths(_initialStageEndsAfterMonths);
    }
}
