using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.PasswordRecoveryTemplates;

namespace YaeaY.Account.Infrastructure.Data.Mappings.PasswordRecoveryTemplates;

public sealed class PasswordRecoveryTemplateMap : IEntityTypeConfiguration<PasswordRecoveryTemplate>
{
    public void Configure(EntityTypeBuilder<PasswordRecoveryTemplate> builder)
    {
        builder.ToTable("PasswordRecoveryTemplates");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Purpose).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.OwnsOne(item => item.FromEmail, email =>
            email.Property(value => value.EmailAddress).HasColumnName("FromEmail").HasMaxLength(254).IsRequired());
        builder.Navigation(item => item.FromEmail).IsRequired();
        builder.Property(item => item.FromName).HasMaxLength(150).IsRequired();
        builder.Property(item => item.Subject).HasMaxLength(200).IsRequired();
        builder.Property(item => item.BodyHtml).HasColumnType("text").IsRequired();
        builder.Property(item => item.IsActive).IsRequired();
        builder.Property(item => item.UpdatedAt).IsRequired();
        builder.HasIndex(item => new { item.Purpose, item.IsActive }).HasDatabaseName("UX_PasswordRecoveryTemplates_Active_Purpose")
            .IsUnique().HasFilter("\"IsActive\" = TRUE");
    }
}
