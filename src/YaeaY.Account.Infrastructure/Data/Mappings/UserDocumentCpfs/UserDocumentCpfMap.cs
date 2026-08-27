using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.UserDocuments;

namespace YaeaY.Account.Infrastructure.Data.Mappings.UserDocumentCpfs;

public sealed class UserDocumentCpfMap : IEntityTypeConfiguration<UserDocumentCpf>
{
    public void Configure(EntityTypeBuilder<UserDocumentCpf> builder)
    {
        builder.ToTable("UserDocumentCpf");
        builder.HasKey(document => document.Id);

        builder.Property<Guid>("UserDocumentId")
            .HasColumnName("UserDocumentId")
            .IsRequired();

        builder.HasIndex("UserDocumentId")
            .IsUnique()
            .HasDatabaseName("UX_UserDocumentCpf_UserDocumentId");

        builder.OwnsOne(document => document.Cpf, cpf =>
        {
            cpf.Property(value => value.Number)
                .HasColumnName("Number")
                .HasMaxLength(11)
                .IsRequired();

            cpf.HasIndex(value => value.Number)
                .HasDatabaseName("IX_UserDocumentCpf_Number");
        });

        builder.Navigation(document => document.Cpf)
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
