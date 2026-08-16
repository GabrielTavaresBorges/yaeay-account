using Microsoft.Extensions.DependencyInjection;
using YaeaY.Account.Application.Services.TelephoneNumbers;

using YaeaY.Account.Application.Services.Emails;

namespace YaeaY.Account.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddTelephoneNumbers();
        services.AddSingleton<EmailConfirmationMessageComposer>();
        services.AddSingleton<EmailAddressMasker>();

        return services;
    }
}
