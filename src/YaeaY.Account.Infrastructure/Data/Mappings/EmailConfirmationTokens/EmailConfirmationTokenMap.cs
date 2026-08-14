using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTokens;

namespace YaeaY.Account.Infrastructure.Data.Mappings.EmailConfirmationTokens;

public sealed class EmailConfirmationTokenMap : IEntityTypeConfiguration<EmailConfirmationToken>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationToken> builder)
    {
        builder.ToTable("EmailConfirmationTokens", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_EmailConfirmationTokens_Expiration",
                "\"ExpiresAt\" > \"CreatedAt\"");

            tableBuilder.HasCheckConstraint(
                "CK_EmailConfirmationTokens_UsedAt",
                "\"UsedAt\" IS NULL OR (\"UsedAt\" >= \"CreatedAt\" AND \"UsedAt\" < \"ExpiresAt\")");

            tableBuilder.HasCheckConstraint(
                "CK_EmailConfirmationTokens_Invalidation",
                "(\"InvalidatedAt\" IS NULL AND \"InvalidationReason\" IS NULL) OR " +
                "(\"InvalidatedAt\" IS NOT NULL AND \"InvalidationReason\" IS NOT NULL AND \"InvalidatedAt\" >= \"CreatedAt\")");

            tableBuilder.HasCheckConstraint(
                "CK_EmailConfirmationTokens_FinalState",
                "\"UsedAt\" IS NULL OR \"InvalidatedAt\" IS NULL");
        });

        builder.HasKey(h => h.Id);

        // === UserId ===
        builder.Property(p => p.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.HasIndex(h => new { h.UserId, h.CreatedAt })
            .HasDatabaseName("IX_EmailConfirmationTokens_UserId_CreatedAt")
            .IsDescending(false, true);

        builder.HasIndex(h => h.UserId)
            .HasDatabaseName("UX_EmailConfirmationTokens_Usable_UserId")
            .IsUnique()
            .HasFilter("\"UsedAt\" IS NULL AND \"InvalidatedAt\" IS NULL");

        builder.HasIndex(h => new { h.ExpiresAt, h.UserId })
            .HasDatabaseName("IX_EmailConfirmationTokens_Pending_ExpiresAt")
            .HasFilter("\"UsedAt\" IS NULL AND \"InvalidatedAt\" IS NULL");

        // ===== Email (VO) =====
        builder.OwnsOne(o => o.Email, email =>
        {
            email.Property(p => p.EmailAddress)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();
        });

        builder.Navigation(n => n.Email)
            .IsRequired();

        // ===== GeneratedEmailConfirmationToken (VO) =====
        builder.OwnsOne(o => o.TokenHash, tokenHash =>
        {
            tokenHash.Property(p => p.Token)
                .HasColumnName("TokenHash")
                .HasMaxLength(1024)
                .IsRequired();

            tokenHash.HasIndex(p => p.Token)
                .HasDatabaseName("UX_EmailConfirmationTokens_TokenHash")
                .IsUnique();
        });

        builder.Navigation(n => n.TokenHash)
            .IsRequired();

        // ===== CreatedAt =====
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // ===== ExpiresAt =====
        builder.Property(p => p.ExpiresAt)
            .HasColumnName("ExpiresAt")
            .IsRequired();

        // ===== UsedAt =====
        builder.Property(p => p.UsedAt)
            .HasColumnName("UsedAt");

        // ===== Invalidation =====
        builder.Property(p => p.InvalidatedAt)
            .HasColumnName("InvalidatedAt");

        builder.Property(p => p.InvalidationReason)
            .HasColumnName("InvalidationReason")
            .HasConversion<string>()
            .HasMaxLength(50);

        // ===== Request audit =====
        builder.Property(p => p.RequestedBy)
            .HasColumnName("RequestedBy")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.RequestReason)
            .HasColumnName("RequestReason")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}
