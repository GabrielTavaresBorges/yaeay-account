using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using YaeaY.Account.Application.Services.OutboxMessages.Interfaces;
using YaeaY.Account.Application.Services.Emails.Interfaces;
using YaeaY.Account.Application.Services.Scheduling.Interfaces;
using YaeaY.Account.Application.Services.Security.Interfaces;
using YaeaY.Account.Application.Services.Identity.Interfaces;
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
using YaeaY.Account.Infrastructure.Identity.Configurations;
using YaeaY.Account.Infrastructure.Identity.Models;
using YaeaY.Account.Infrastructure.Identity.Services;
using YaeaY.Account.Infrastructure.Messaging.Outbox;
using YaeaY.Account.Infrastructure.Scheduling.Quartz;
using YaeaY.Account.Infrastructure.Services.TelephoneNumbers.Libraries.LibPhoneNumber;
using YaeaY.Account.Infrastructure.Services.Emails;
using YaeaY.Account.Infrastructure.Services.Emails.Smtp;

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
        services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
        services.AddScoped<IEmailConfirmationTemplateRepository, EmailConfirmationTemplateRepository>();

        // Security and identity
        services.AddOptions<AccountSessionOptions>()
            .Bind(configuration.GetRequiredSection(AccountSessionOptions.SectionName))
            .Validate(options => options.IdleTimeoutInMinutes is > 0 and <= 1440,
                "Authentication:Session:IdleTimeoutInMinutes must be between 1 and 1440.")
            .Validate(options => options.RememberMeDurationInDays is > 0 and <= 90,
                "Authentication:Session:RememberMeDurationInDays must be between 1 and 90.")
            .Validate(options => options.SecurityStampValidationIntervalInMinutes is > 0 and <= 60,
                "Authentication:Session:SecurityStampValidationIntervalInMinutes must be between 1 and 60.")
            .Validate(options => options.MaxFailedAccessAttempts is > 0 and <= 20,
                "Authentication:Session:MaxFailedAccessAttempts must be between 1 and 20.")
            .Validate(options => options.LockoutDurationInMinutes is > 0 and <= 1440,
                "Authentication:Session:LockoutDurationInMinutes must be between 1 and 1440.")
            .ValidateOnStart();

        var sessionOptions = configuration
            .GetRequiredSection(AccountSessionOptions.SectionName)
            .Get<AccountSessionOptions>()
            ?? throw new InvalidOperationException("Authentication session configuration is invalid.");

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = sessionOptions.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                    sessionOptions.LockoutDurationInMinutes);
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = "__Host-YaeaY.Account";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = "/";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionOptions.IdleTimeoutInMinutes);
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(
                sessionOptions.SecurityStampValidationIntervalInMinutes));

        services.AddScoped<IIdentityAccountService, IdentityAccountService>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<
            IEmailConfirmationTokenExpirationPolicy,
            ConfigurationEmailConfirmationTokenExpirationPolicy>();

        // External service adapters
        services.AddSingleton<ITelephoneNumberService, LibPhoneNumberService>();
        services.AddScoped<IEmailConfirmationTokenService, EmailConfirmationTokenService>();

        // Email delivery
        services.AddOptions<EmailConfirmationLinkOptions>()
            .Bind(configuration.GetRequiredSection(
                EmailConfirmationLinkOptions.SectionName))
            .Validate(
                options => ConfiguredEmailConfirmationLinkBuilder
                    .IsValidConfirmationPageUrl(options.ConfirmationPageUrl),
                "EmailConfirmationLink:ConfirmationPageUrl must be an absolute HTTPS URL without query or fragment.")
            .ValidateOnStart();

        services.AddSingleton<
            IEmailConfirmationLinkBuilder,
            ConfiguredEmailConfirmationLinkBuilder>();

        services.AddOptions<SmtpEmailOptions>()
            .Bind(configuration.GetRequiredSection(SmtpEmailOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "EmailDelivery:Smtp:Host is required.")
            .Validate(
                options => options.Port is > 0 and <= 65535,
                "EmailDelivery:Smtp:Port must be between 1 and 65535.")
            .Validate(
                options => Enum.IsDefined(options.SecurityMode),
                "EmailDelivery:Smtp:SecurityMode is invalid.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Username),
                "EmailDelivery:Smtp:Username is required.")
            .Validate(
                options => options.TimeoutInSeconds is > 0 and <= 300,
                "EmailDelivery:Smtp:TimeoutInSeconds must be between 1 and 300.")
            .Validate(
                options => !options.IsActive || !string.IsNullOrWhiteSpace(options.Password),
                "EmailDelivery:Smtp:Password is required when SMTP delivery is active.")
            .ValidateOnStart();

        services.AddScoped<IEmailSender, HostingerSmtpEmailSender>();

        // Domain event dispatching
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<MediatRDomainEventPublisher>();

        // Scheduling
        services.AddOptions<OutboxProcessingScheduleOptions>()
            .Bind(configuration.GetRequiredSection(OutboxProcessingScheduleOptions.SectionName))
            .Validate(
                options => options.IntervalInSeconds > 0,
                "Scheduling:OutboxProcessing:IntervalInSeconds must be positive.")
            .Validate(
                options => options.BatchSize > 0,
                "Scheduling:OutboxProcessing:BatchSize must be positive.")
            .Validate(
                options => options.RetryDelayInSeconds > 0,
                "Scheduling:OutboxProcessing:RetryDelayInSeconds must be positive.")
            .ValidateOnStart();

        services.AddQuartz(quartz =>
        {
            var jobKey = new JobKey(
                QuartzJobKeys.ProcessOutboxMessages,
                QuartzJobKeys.Group);

            quartz.AddJob<ProcessOutboxMessagesJob>(job =>
                job.WithIdentity(jobKey).StoreDurably());
        });

        services.AddQuartzHostedService(options =>
            options.WaitForJobsToComplete = true);

        services.AddSingleton<IJobScheduler, QuartzJobScheduler>();
        services.AddHostedService<QuartzSchedulingHostedService>();

        return services;
    }
}
