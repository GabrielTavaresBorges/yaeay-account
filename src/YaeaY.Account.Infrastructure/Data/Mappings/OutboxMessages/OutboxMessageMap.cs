using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YaeaY.Account.Domain.Entities.AggregateRoots.OutboxMessages;
using YaeaY.Account.Domain.ValueObjects.Events;

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

        builder.OwnsOne(message => message.Content, content =>
        {
            content.Property(serializedEvent => serializedEvent.EventType)
                .HasField("_eventType")
                .HasColumnName("EventType")
                .HasMaxLength(SerializedDomainEvent.EventTypeMaximumLength)
                .IsRequired();

            content.Property(serializedEvent => serializedEvent.Payload)
                .HasField("_payload")
                .HasColumnName("Payload")
                .HasColumnType("jsonb")
                .IsRequired();
        });

        builder.Navigation(message => message.Content)
            .HasField("_content")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(message => message.OccurredOnUtc)
            .HasField("_occurredOnUtc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(message => message.ProcessedOnUtc)
            .HasField("_processedOnUtc")
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.LastAttemptOnUtc)
            .HasField("_lastAttemptOnUtc")
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.NextAttemptOnUtc)
            .HasField("_nextAttemptOnUtc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(message => message.AttemptCount)
            .HasField("_attemptCount")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasField("_lastError")
            .HasColumnType("text");

        builder.Ignore(message => message.IsProcessed);

        builder.HasIndex(message => new
            {
                message.NextAttemptOnUtc,
                message.OccurredOnUtc
            })
            .HasDatabaseName("IX_OutboxMessages_Pending")
            .HasFilter("\"ProcessedOnUtc\" IS NULL");
    }
}
