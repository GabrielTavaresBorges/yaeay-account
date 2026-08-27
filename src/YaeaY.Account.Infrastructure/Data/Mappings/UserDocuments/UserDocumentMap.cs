using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.UserDocuments;

namespace YaeaY.Account.Infrastructure.Data.Mappings.UserDocuments;

public sealed class UserDocumentMap : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(EntityTypeBuilder<UserDocument> builder)
    {
        builder.ToTable("UserDocuments");
        builder.HasKey(h => h.Id);

        // === UserId ===
        builder.Property<Guid>("UserId")
            .HasColumnName("UserId")
            .IsRequired();
        builder.HasIndex("UserId");

        // ===== DocumentType =====
        builder.Property(p => p.DocumentType)
            .HasColumnName("DocumentType")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IssuerCountry)
            .HasColumnName("IssuerCountry")
            .HasColumnType("character(2)")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(p => p.IsVerified)
            .HasColumnName("IsVerified")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.VerifiedAt)
            .HasColumnName("VerifiedAt");

        // ===== CreatedAt =====
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.HasOne(document => document.Cpf)
            .WithOne()
            .HasForeignKey<UserDocumentCpf>("UserDocumentId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(document => document.Cpf)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(document => document.Images)
            .WithOne()
            .HasForeignKey("UserDocumentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(document => document.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
