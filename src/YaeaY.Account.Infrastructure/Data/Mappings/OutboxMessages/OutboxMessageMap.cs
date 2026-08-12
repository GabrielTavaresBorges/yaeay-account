using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Infrastructure.Messaging.Outbox;

namespace YaeaY.Account.Infrastructure.Data.Mappings.OutboxMessages;

public sealed class OutboxMessageMap : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable(
            "OutboxMessages",
            table => table.HasCheckConstraint(
                "CK_OutboxMessages_AttemptCount",
                "\"AttemptCount\" >= 0"));
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.OccurredOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(message => message.ProcessedOnUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.LastAttemptOnUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.NextAttemptOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(message => message.AttemptCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasColumnType("text");

        builder.HasIndex(message => new
            {
                message.NextAttemptOnUtc,
                message.OccurredOnUtc
            })
            .HasDatabaseName("IX_OutboxMessages_Pending")
            .HasFilter("\"ProcessedOnUtc\" IS NULL");
    }
}
