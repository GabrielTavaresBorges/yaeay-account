using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.UserDocuments;

namespace YaeaY.Account.Infrastructure.Data.Mappings.UserDocumentRgs;

public sealed class UserDocumentRgMap : IEntityTypeConfiguration<UserDocumentRg>
{
    public void Configure(EntityTypeBuilder<UserDocumentRg> builder)
    {
        builder.ToTable("UserDocumentRg");
        builder.HasKey(document => document.Id);

        builder.Property<Guid>("UserDocumentId")
            .HasColumnName("UserDocumentId")
            .IsRequired();

        builder.HasIndex("UserDocumentId")
            .IsUnique()
            .HasDatabaseName("UX_UserDocumentRg_UserDocumentId");

        builder.OwnsOne(document => document.Rg, rg =>
        {
            rg.Property(value => value.Number)
                .HasColumnName("IdentityNumber")
                .HasMaxLength(30)
                .IsRequired();

            rg.Property(value => value.IssuedAt)
                .HasColumnName("IssuedAt")
                .IsRequired();

            rg.Property(value => value.IssuingAuthority)
                .HasColumnName("IssuingAuthority")
                .HasMaxLength(100)
                .IsRequired();

            rg.Property(value => value.IssuingState)
                .HasColumnName("IssuingState")
                .HasColumnType("character(2)")
                .HasMaxLength(2)
                .IsRequired();

            rg.HasIndex(value => new { value.Number, value.IssuingState })
                .HasDatabaseName("IX_UserDocumentRg_IdentityNumber_IssuingState");
        });

        builder.Navigation(document => document.Rg).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
