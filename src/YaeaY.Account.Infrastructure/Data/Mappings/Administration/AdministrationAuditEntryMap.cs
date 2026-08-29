using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.Administration;

namespace YaeaY.Account.Infrastructure.Data.Mappings.Administration;

public sealed class AdministrationAuditEntryMap : IEntityTypeConfiguration<AdministrationAuditEntry>
{
    public void Configure(EntityTypeBuilder<AdministrationAuditEntry> builder)
    {
        builder.ToTable("AdministrationAuditEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.AdministratorId).IsRequired();
        builder.Property(entry => entry.TargetUserId);
        builder.Property(entry => entry.Action).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.Justification).HasMaxLength(500).IsRequired();
        builder.Property(entry => entry.OccurredAtUtc).IsRequired();
        builder.HasIndex(entry => entry.OccurredAtUtc);
        builder.HasIndex(entry => entry.TargetUserId);
    }
}
