using YaeaY.Account.Application;
using YaeaY.Account.Infrastructure;
using YaeaY.Account.Infrastructure.Data.Context;
using YaeaY.Account.Infrastructure.Messaging.RabbitMq;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAuthorization(options =>
    options.AddPolicy("AccountAdministration", policy => policy.RequireRole("Admin")));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("password-recovery", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-YaeaY-CSRF";
    options.Cookie.Name = "__Host-YaeaY.Account.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
});

var dataProtectionKeyRingPath = builder.Configuration["DataProtection:KeyRingPath"];
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "YaeaY.Account");

if (!string.IsNullOrWhiteSpace(dataProtectionKeyRingPath))
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyRingPath));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // The API is reachable only through the internal Docker network in deployments.
    // Clearing the defaults permits the reverse proxy network to forward HTTPS correctly.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add application services
builder.Services.AddApplication();
// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

if (builder.Environment.IsDevelopment())
    builder.Services.AddHostedService<RabbitMqEventConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await database.Database.ExecuteSqlRawAsync("""
        ALTER TABLE account_write."OutboxMessages"
            ADD COLUMN IF NOT EXISTS "PublishedOnUtc" TIMESTAMP WITH TIME ZONE,
            ADD COLUMN IF NOT EXISTS "LastPublishAttemptOnUtc" TIMESTAMP WITH TIME ZONE,
            ADD COLUMN IF NOT EXISTS "NextPublishAttemptOnUtc" TIMESTAMP WITH TIME ZONE,
            ADD COLUMN IF NOT EXISTS "PublishAttemptCount" INTEGER NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS "LastPublishError" TEXT;

        UPDATE account_write."OutboxMessages"
        SET "NextPublishAttemptOnUtc" = "OccurredOnUtc"
        WHERE "NextPublishAttemptOnUtc" IS NULL;

        ALTER TABLE account_write."OutboxMessages"
            ALTER COLUMN "NextPublishAttemptOnUtc" SET NOT NULL;

        ALTER TABLE account_write."OutboxMessages"
            DROP CONSTRAINT IF EXISTS "CK_OutboxMessages_PublishAttemptCount";

        ALTER TABLE account_write."OutboxMessages"
            ADD CONSTRAINT "CK_OutboxMessages_PublishAttemptCount"
            CHECK ("PublishAttemptCount" >= 0);

        CREATE INDEX IF NOT EXISTS "IX_OutboxMessages_PendingPublication"
            ON account_write."OutboxMessages" ("NextPublishAttemptOnUtc", "OccurredOnUtc")
            WHERE "ProcessedOnUtc" IS NOT NULL
              AND "PublishedOnUtc" IS NULL;

        CREATE INDEX IF NOT EXISTS "IX_OutboxMessages_UserProfileFreshness"
            ON account_write."OutboxMessages" (("Payload" ->> 'UserId'), "OccurredOnUtc" DESC)
            WHERE "EventType" = 'YaeaY.Account.Domain.Events.Users.UserProfileChangedDomainEvent';
        """);
}

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "YaeaY Account API | v1");
    }); 
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
