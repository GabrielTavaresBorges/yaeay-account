using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Enumerators;
using YaeaY.Account.Domain.ValueObjects.Accounts;

namespace YaeaY.Account.Infrastructure.Data.Mappings.Users;

public sealed class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(h => h.Id);

        // ===== Email (VO) =====
        builder.OwnsOne(o => o.Email, email =>
        {
            email.Property(p => p.EmailAddress)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();

            email.HasIndex(p => p.EmailAddress)
                .IsUnique()
                .HasDatabaseName("UX_User_Email");
        });

        builder.Navigation(n => n.Email)
            .IsRequired();

        // ===== PasswordHash (VO) =====
        builder.OwnsOne(o => o.PasswordHash, pw =>
        {
            pw.Property(p => p.Password)
              .HasColumnName("PasswordHash")
              .HasMaxLength(1024)
              .IsRequired();
        });

        builder.Navigation(n => n.PasswordHash).IsRequired();

        // ===== FullName (VO) =====
        builder.OwnsOne(o => o.FullName, name =>
        {
            name.Property(p => p.Name)
                .HasColumnName("UserName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Navigation(n => n.FullName)
            .IsRequired();

        // ===== BirthDate (VO) =====
        builder.OwnsOne(o => o.BirthDate, bd =>
        {
            bd.Property(p => p.Date)
              .HasColumnName("BirthDate")
              .HasColumnType("date")
              .IsRequired();
        });

        builder.Navigation(n => n.BirthDate).IsRequired();

        // ===== Status (enum) =====
        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // ===== Gender (enum) =====
        builder.Property(p => p.Gender)
            .HasColumnName("Gender")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // ===== CreatedAt =====
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(p => p.EmailConfirmedAt)
            .HasColumnName("EmailConfirmedAt");

        builder.Property(p => p.FirstLoginAt)
            .HasColumnName("FirstLoginAt");

        builder.Property(p => p.LastLoginAt)
            .HasColumnName("LastLoginAt");

        // ===== SuspensionInfo (nullable) =====
        builder.OwnsOne(typeof(SuspensionInfo), "_suspension", si =>
        {
            si.Property(nameof(SuspensionInfo.Reason))
            .HasColumnName("SuspensionReason")
            .HasConversion<int>();

            si.Property(nameof(SuspensionInfo.SuspensionBy))
            .HasColumnName("SuspensionBy")
            .HasConversion<int>();

            si.Property(nameof(SuspensionInfo.SuspendedAt))
            .HasColumnName("SuspendedAt");

            si.Property(nameof(SuspensionInfo.SuspendedUntil))
            .HasColumnName("SuspendedUntil");

            si.Property(nameof(SuspensionInfo.Note))
            .HasColumnName("SuspensionNote")
            .HasMaxLength(500);
        });

        builder.Navigation("_suspension")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ===== Phones =====
        builder.HasMany(user => user.Phones)
            .WithOne()
            .HasForeignKey("UserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Phones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ===== Documents =====
        builder.HasMany(user => user.Documents)
            .WithOne()
            .HasForeignKey("UserId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
