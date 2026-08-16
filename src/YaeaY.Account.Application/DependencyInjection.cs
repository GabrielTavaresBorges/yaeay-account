using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using YaeaY.Account.Application.Behaviors;
using YaeaY.Account.Application.Services.Emails;
using YaeaY.Account.Application.Services.TelephoneNumbers;

namespace YaeaY.Account.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTelephoneNumbers();
        services.AddSingleton<EmailConfirmationMessageComposer>();
        services.AddSingleton<EmailAddressMasker>();

        return services;
    }
}
