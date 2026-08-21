using Microsoft.Extensions.Options;
using YaeaY.Account.Domain.Policies.PasswordRecoveries;
using YaeaY.Account.Infrastructure.Identity.Configurations;

namespace YaeaY.Account.Infrastructure.Identity.Policies;

public sealed class ConfigurationPasswordRecoveryPolicy(IOptions<PasswordRecoveryOptions> options)
    : IPasswordRecoveryPolicy
{
    public TimeSpan CodeLifetime => TimeSpan.FromMinutes(options.Value.CodeLifetimeInMinutes);
    public TimeSpan ResetAuthorizationLifetime => TimeSpan.FromMinutes(options.Value.ResetAuthorizationLifetimeInMinutes);
    public TimeSpan ResendInterval => TimeSpan.FromSeconds(options.Value.ResendIntervalInSeconds);
    public TimeSpan RequestWindow => TimeSpan.FromMinutes(options.Value.RequestWindowInMinutes);
    public int MaximumFailedAttempts => options.Value.MaximumFailedAttempts;
    public int MaximumRequestsPerWindow => options.Value.MaximumRequestsPerWindow;
}
