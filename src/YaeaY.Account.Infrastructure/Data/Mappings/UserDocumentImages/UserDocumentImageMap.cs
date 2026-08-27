using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.UserDocuments;

namespace YaeaY.Account.Infrastructure.Data.Mappings.UserDocumentImages;

public sealed class UserDocumentImageMap : IEntityTypeConfiguration<UserDocumentImage>
{
    public void Configure(EntityTypeBuilder<UserDocumentImage> builder)
    {
        builder.ToTable("UserDocumentImages");
        builder.HasKey(image => image.Id);

        builder.Property<Guid>("UserDocumentId")
            .HasColumnName("UserDocumentId")
            .IsRequired();

        builder.Property(image => image.Position)
            .HasColumnName("Position")
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(image => image.StorageObjectKey)
            .HasColumnName("StorageObjectKey")
            .HasMaxLength(UserDocumentImage.MaximumStorageObjectKeyLength)
            .IsRequired();

        builder.Property(image => image.OriginalFileName)
            .HasColumnName("OriginalFileName")
            .HasMaxLength(UserDocumentImage.MaximumOriginalFileNameLength)
            .IsRequired();

        builder.Property(image => image.ContentType)
            .HasColumnName("ContentType")
            .HasMaxLength(UserDocumentImage.MaximumContentTypeLength)
            .IsRequired();

        builder.Property(image => image.FileSizeBytes)
            .HasColumnName("FileSizeBytes")
            .IsRequired();

        builder.Property(image => image.Sha256Hash)
            .HasColumnName("Sha256Hash")
            .HasColumnType("character(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(image => image.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.HasIndex("UserDocumentId", nameof(UserDocumentImage.Position))
            .IsUnique()
            .HasDatabaseName("UX_UserDocumentImages_UserDocumentId_Position");

        builder.HasIndex(image => image.StorageObjectKey)
            .IsUnique()
            .HasDatabaseName("UX_UserDocumentImages_StorageObjectKey");
    }
}
