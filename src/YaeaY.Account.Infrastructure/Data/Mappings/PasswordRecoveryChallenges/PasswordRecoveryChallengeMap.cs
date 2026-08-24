using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryChallenges;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;

namespace YaeaY.Account.Infrastructure.Data.Mappings.PasswordRecoveryChallenges;

public sealed class PasswordRecoveryChallengeMap : IEntityTypeConfiguration<PasswordRecoveryChallenge>
{
    public void Configure(EntityTypeBuilder<PasswordRecoveryChallenge> builder)
    {
        builder.ToTable("PasswordRecoveryChallenges", table =>
        {
            table.HasCheckConstraint("CK_PasswordRecoveryChallenges_Issuance", "(\"CodeHash\" IS NULL AND \"IssuedAt\" IS NULL AND \"ExpiresAt\" IS NULL) OR (\"CodeHash\" IS NOT NULL AND \"IssuedAt\" IS NOT NULL AND \"ExpiresAt\" > \"IssuedAt\")");
            table.HasCheckConstraint("CK_PasswordRecoveryChallenges_Verification", "(\"VerifiedAt\" IS NULL AND \"AuthorizationExpiresAt\" IS NULL) OR (\"VerifiedAt\" IS NOT NULL AND \"AuthorizationExpiresAt\" > \"VerifiedAt\")");
            table.HasCheckConstraint("CK_PasswordRecoveryChallenges_Invalidation", "(\"InvalidatedAt\" IS NULL AND \"InvalidationReason\" IS NULL) OR (\"InvalidatedAt\" IS NOT NULL AND \"InvalidationReason\" IS NOT NULL)");
            table.HasCheckConstraint("CK_PasswordRecoveryChallenges_FinalState", "NOT (\"ConsumedAt\" IS NOT NULL AND \"InvalidatedAt\" IS NOT NULL)");
            table.HasCheckConstraint("CK_PasswordRecoveryChallenges_FailedAttempts", "\"FailedAttempts\" >= 0");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.UserId).IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(item => item.RequestedAt).IsRequired();
        builder.Property(item => item.IssuedAt);
        builder.Property(item => item.ExpiresAt);
        builder.Property(item => item.FailedAttempts).IsRequired();
        builder.Property(item => item.VerifiedAt);
        builder.Property(item => item.AuthorizationExpiresAt);
        builder.Property(item => item.ConsumedAt);
        builder.Property(item => item.InvalidatedAt);
        builder.Property(item => item.InvalidationReason).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(item => item.Email, email =>
            email.Property(value => value.EmailAddress).HasColumnName("Email").HasMaxLength(254).IsRequired());
        builder.Navigation(item => item.Email).IsRequired();

        builder.OwnsOne(item => item.CodeHash, hash =>
        {
            hash.Property(value => value.Value).HasColumnName("CodeHash").HasMaxLength(128);
            hash.HasIndex(value => value.Value).HasDatabaseName("UX_PasswordRecoveryChallenges_CodeHash").IsUnique().HasFilter("\"CodeHash\" IS NOT NULL");
        });

        builder.HasIndex(item => new { item.UserId, item.RequestedAt })
            .HasDatabaseName("IX_PasswordRecoveryChallenges_UserId_RequestedAt").IsDescending(false, true);
        builder.HasIndex(item => item.UserId).HasDatabaseName("UX_PasswordRecoveryChallenges_Open_UserId")
            .IsUnique().HasFilter("\"ConsumedAt\" IS NULL AND \"InvalidatedAt\" IS NULL");
        builder.HasIndex(item => new { item.ExpiresAt, item.UserId }).HasDatabaseName("IX_PasswordRecoveryChallenges_Open_ExpiresAt")
            .HasFilter("\"ConsumedAt\" IS NULL AND \"InvalidatedAt\" IS NULL");
    }
}
