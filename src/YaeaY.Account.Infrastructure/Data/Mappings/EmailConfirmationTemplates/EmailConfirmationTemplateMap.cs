using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.EmailConfirmationTemplates;

namespace YaeaY.Account.Infrastructure.Data.Mappings.EmailConfirmationTemplates;

public sealed class EmailConfirmationTemplateMap : IEntityTypeConfiguration<EmailConfirmationTemplate>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationTemplate> builder)
    {
        builder.ToTable("EmailConfirmationTemplate");
        builder.HasKey(h => h.Id);

        // ===== FromEmail (VO) =====
        builder.OwnsOne(o => o.FromEmail, email =>
        {
            email.Property(p => p.EmailAddress)
                .HasColumnName("FromEmail")
                .HasColumnType("varchar(254)")
                .HasMaxLength(254)
                .IsRequired();
        });

        builder.Navigation(n => n.FromEmail)
            .IsRequired();

        // ===== FromName =====
        builder.Property(p => p.FromName)
            .HasColumnName("FromName")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        // ===== Subject =====
        builder.Property(p => p.Subject)
            .HasColumnName("Subject")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        // ===== BodyHtml =====
        builder.Property(p => p.BodyHtml)
            .HasColumnName("BodyHtml")
            .HasColumnType("text")
            .IsRequired();

        // ===== IsActive =====
        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("boolean")
            .IsRequired();

        // ===== UpdatedAt =====
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
