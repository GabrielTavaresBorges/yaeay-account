using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Application.Services.TelephoneNumbers.Interfaces;
using YaeaY.Account.Domain.Abstraction.Interfaces;
using YaeaY.Account.Domain.Policies.EmailConfirmations;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTemplates;
using YaeaY.Account.Domain.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Data.Persistence;
using YaeaY.Account.Infrastructure.Data.Repositories.EmailConfirmationTemplates;
using YaeaY.Account.Infrastructure.Data.Repositories.EmailConfirmationTokens;
using YaeaY.Account.Infrastructure.Data.Repositories.Users;
using YaeaY.Account.Infrastructure.Events.Dispatchers;
using YaeaY.Account.Infrastructure.Events.Publishers;
using YaeaY.Account.Infrastructure.Identity.Policies;
using YaeaY.Account.Infrastructure.Identity.Securities;
using YaeaY.Account.Infrastructure.Identity.Services;
using YaeaY.Account.Infrastructure.Messaging.Outbox;
using YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber;

namespace YaeaY.Account.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não encontrada. Verifique appsettings.json (Presentation.Server).");

        // Persistence
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDomainEventSerializer, JsonDomainEventSerializer>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
        services.AddScoped<IEmailConfirmationTemplateRepository, EmailConfirmationTemplateRepository>();

        // Security and identity
        services.AddScoped<IPasswordHasher, AspNetIdentityPasswordHasher>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<
            IEmailConfirmationTokenExpirationPolicy,
            ConfigurationEmailConfirmationTokenExpirationPolicy>();

        // External service adapters
        services.AddSingleton<ITelephoneNumberService, LibPhoneNumberService>();
        services.AddScoped<IEmailConfirmationTokenService, EmailConfirmationTokenService>();

        // Email delivery
        //services.AddScoped<IEmailSender, EmailSender>();

        // Domain event dispatching
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<MediatRDomainEventPublisher>();

        return services;
    }
}
