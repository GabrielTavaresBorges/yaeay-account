using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationSettings;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;

namespace YaeaY.Account.Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
    public DbSet<EmailConfirmationSetting> EmailConfirmationSettings => Set<EmailConfirmationSetting>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
