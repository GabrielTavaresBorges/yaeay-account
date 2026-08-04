using Microsoft.Extensions.DependencyInjection;
using YaeaY.Account.Domain.Factories.Telephones;
using YaeaY.Account.Domain.Policies.Telephones.Countries.Brazil;
using YaeaY.Account.Domain.Policies.Telephones.Countries.Interfaces;

namespace YaeaY.Account.Application.Services.TelephoneNumbers;

internal static class TelephoneNumberRegistration
{
    internal static IServiceCollection AddTelephoneNumbers(this IServiceCollection services)
    {
        services.AddTransient<ITelephoneNumberFactory, TelephoneNumberFactory>();
        services.AddTransient<ITelephoneNumberCountryPolicy, BrazilTelephoneNumberPolicy>();

        return services;
    }
}
