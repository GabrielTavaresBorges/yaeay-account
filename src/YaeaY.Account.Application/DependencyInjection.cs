using Microsoft.Extensions.DependencyInjection;
using YaeaY.Account.Application.Services.TelephoneNumbers;

namespace YaeaY.Account.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddTelephoneNumbers();

        return services;
    }
}