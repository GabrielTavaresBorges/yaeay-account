using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Infrastructure.Identity.Models;

namespace YaeaY.Account.Infrastructure.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
    public DbSet<EmailConfirmationTemplate> EmailConfirmationTemplates => Set<EmailConfirmationTemplate>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PasswordRecoveryChallenge> PasswordRecoveryChallenges => Set<PasswordRecoveryChallenge>();
    public DbSet<PasswordRecoveryTemplate> PasswordRecoveryTemplates => Set<PasswordRecoveryTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("account_write");

        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.ToTable("IdentityUsers");
            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<ApplicationUser>(identity => identity.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationRole>().ToTable("IdentityRoles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("IdentityUserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("IdentityUserClaims");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("IdentityRoleClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("IdentityUserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("IdentityUserTokens");
        modelBuilder.Ignore<IdentityUserPasskey<Guid>>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
